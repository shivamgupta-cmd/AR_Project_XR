import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

WIDTH = 4096
HEIGHT = 2048
ROOT = Path("GlobeRegionHighlight_Unity2022/Assets/GlobeRegionHighlight")
COUNTRIES = ROOT / "SourceData/ne_110m_admin_0_countries.geojson"
LAND = ROOT / "SourceData/ne_110m_land.geojson"
TEXTURES = ROOT / "Textures"
MASKS = TEXTURES / "Masks"

REGIONS = {
    "Asia": 1,
    "Africa": 2,
    "North America": 3,
    "South America": 4,
    "Europe": 5,
    "Oceania": 6,
    "Antarctica": 7,
    "Pacific Ocean": 8,
    "Atlantic Ocean": 9,
    "Indian Ocean": 10,
    "Arctic Ocean": 11,
    "Southern Ocean": 12,
}

CONTINENT_COLORS = {
    1: (232, 177, 67),
    2: (94, 178, 91),
    3: (220, 103, 87),
    4: (45, 174, 146),
    5: (154, 116, 202),
    6: (235, 132, 62),
    7: (211, 235, 243),
}

OCEAN_COLORS = {
    8: (15, 79, 142),
    9: (20, 97, 156),
    10: (19, 113, 164),
    11: (73, 147, 183),
    12: (47, 122, 165),
}


def map_xy(lon, lat):
    return ((lon + 180.0) / 360.0 * WIDTH, (90.0 - lat) / 180.0 * HEIGHT)


def unwrap_ring(ring):
    if not ring:
        return []
    result = [list(ring[0])]
    previous = ring[0][0]
    offset = 0.0
    for lon, lat in ring[1:]:
        candidate = lon + offset
        while candidate - previous > 180.0:
            offset -= 360.0
            candidate = lon + offset
        while candidate - previous < -180.0:
            offset += 360.0
            candidate = lon + offset
        result.append([candidate, lat])
        previous = candidate
    return result


def polygon_sets(geometry):
    kind = geometry.get("type")
    coordinates = geometry.get("coordinates", [])
    if kind == "Polygon":
        yield coordinates
    elif kind == "MultiPolygon":
        yield from coordinates


def draw_wrapped_polygon(draw, polygon, fill, outline=None, width=1, preserve_holes=False):
    if not polygon:
        return
    outer = unwrap_ring(polygon[0])
    holes = [unwrap_ring(ring) for ring in polygon[1:]]
    for shift in (-WIDTH, 0, WIDTH):
        outer_points = [(map_xy(lon, lat)[0] + shift, map_xy(lon, lat)[1]) for lon, lat in outer]
        draw.polygon(outer_points, fill=fill)
        if preserve_holes:
            for hole in holes:
                hole_points = [(map_xy(lon, lat)[0] + shift, map_xy(lon, lat)[1]) for lon, lat in hole]
                draw.polygon(hole_points, fill=0 if isinstance(fill, int) else OCEAN_COLORS[8])
        if outline is not None:
            draw.line(outer_points, fill=outline, width=width, joint="curve")


def create_ocean_ids():
    y = np.arange(HEIGHT)[:, None]
    x = np.arange(WIDTH)[None, :]
    lat = 90.0 - (y + 0.5) / HEIGHT * 180.0
    lon = (x + 0.5) / WIDTH * 360.0 - 180.0
    ids = np.full((HEIGHT, WIDTH), 8, dtype=np.uint8)
    ids[(lat >= 66.5) * np.ones_like(lon, dtype=bool)] = 11
    ids[(lat <= -60.0) * np.ones_like(lon, dtype=bool)] = 12
    middle = (lat < 66.5) & (lat > -60.0)
    atlantic = middle & (lon >= -70.0) & (lon < 20.0)
    indian = middle & (lon >= 20.0) & (lon < 120.0) & (lat < 32.0)
    ids[np.broadcast_to(atlantic, ids.shape)] = 9
    ids[np.broadcast_to(indian, ids.shape)] = 10
    return ids


def create_region_id_map(countries):
    id_image = Image.fromarray(create_ocean_ids(), mode="L")
    draw = ImageDraw.Draw(id_image)
    antarctica_points = []
    for feature in countries["features"]:
        continent = feature.get("properties", {}).get("continent")
        region_id = REGIONS.get(continent)
        if region_id is None:
            continue
        for polygon in polygon_sets(feature["geometry"]):
            if continent == "Antarctica":
                antarctica_points.extend(polygon[0])
            else:
                draw_wrapped_polygon(draw, polygon, fill=region_id)

    # Antarctica surrounds the South Pole and crosses the map seam. Rasterize
    # it as coastline latitude by longitude instead of a flat wrapped polygon.
    if antarctica_points:
        coast = np.full(WIDTH, np.nan, dtype=np.float32)
        for lon, lat in antarctica_points:
            if lat <= -89.0:
                continue
            x = int(((lon + 180.0) % 360.0) / 360.0 * WIDTH) % WIDTH
            coast[x] = lat if np.isnan(coast[x]) else max(coast[x], lat)
        known = np.flatnonzero(~np.isnan(coast))
        extended_x = np.concatenate((known - WIDTH, known, known + WIDTH))
        extended_y = np.concatenate((coast[known], coast[known], coast[known]))
        coast = np.interp(np.arange(WIDTH), extended_x, extended_y)
        # Smooth sparse vertex-to-column jumps while wrapping at the seam.
        window = 81
        half = window // 2
        wrapped = np.concatenate((coast[-half:], coast, coast[:half]))
        coast = np.convolve(wrapped, np.ones(window) / window, mode="valid")
        ids = np.array(id_image)
        latitudes = 90.0 - (np.arange(HEIGHT)[:, None] + 0.5) / HEIGHT * 180.0
        ids[latitudes <= coast[None, :]] = REGIONS["Antarctica"]
        id_image = Image.fromarray(ids.astype(np.uint8), mode="L")

    # Correct overseas territories and the conventional Europe/Asia split for
    # a seven-continent classroom map.
    ids = np.array(id_image)
    land = (ids >= 1) & (ids <= 7)
    y = np.arange(HEIGHT)[:, None]
    x = np.arange(WIDTH)[None, :]
    lat = np.broadcast_to(90.0 - (y + 0.5) / HEIGHT * 180.0, ids.shape)
    lon = np.broadcast_to((x + 0.5) / WIDTH * 360.0 - 180.0, ids.shape)
    south_america = land & (lon >= -82.5) & (lon < -25.0) & (lat <= 13.5) & (lat > -60.0)
    north_america = land & (lon < -25.0) & ~south_america & (lat > -60.0)
    oceania = land & (lon >= 110.0) & (lat < 0.0) & (lat > -60.0)
    europe_asia_boundary = np.interp(lat, [40, 45, 50, 55, 60, 70, 90], [45, 50, 55, 58, 60, 60, 60])
    eurasia_east = land & (ids == REGIONS["Europe"]) & (lon >= europe_asia_boundary) & (lat >= 40.0)
    ids[south_america] = REGIONS["South America"]
    ids[north_america] = REGIONS["North America"]
    ids[oceania] = REGIONS["Oceania"]
    ids[eurasia_east] = REGIONS["Asia"]
    return Image.fromarray(ids.astype(np.uint8), mode="L")


def create_base_texture(region_ids, land):
    ids = np.array(region_ids)
    image = np.zeros((HEIGHT, WIDTH, 3), dtype=np.uint8)
    lat = np.linspace(90.0, -90.0, HEIGHT)[:, None]
    lon = np.linspace(-180.0, 180.0, WIDTH)[None, :]
    ocean_shade = 0.88 + 0.12 * np.cos(np.radians(lat))
    subtle = 1.0 + 0.018 * np.sin(np.radians(lon * 3.0)) * np.cos(np.radians(lat * 2.0))
    shade = ocean_shade * subtle

    # Keep the visible ocean continuous; basin divisions exist only in the
    # invisible Region ID map and appear when highlighted by the shader.
    visible_ocean_color = (15, 83, 145)
    for region_id in OCEAN_COLORS:
        color = visible_ocean_color
        mask = ids == region_id
        for channel in range(3):
            values = np.clip(color[channel] * shade, 0, 255).astype(np.uint8)
            image[:, :, channel][mask] = np.broadcast_to(values, mask.shape)[mask]

    for region_id, color in CONTINENT_COLORS.items():
        mask = ids == region_id
        polar_light = 0.92 + 0.08 * np.cos(np.radians(lat))
        for channel in range(3):
            values = np.clip(color[channel] * polar_light, 0, 255).astype(np.uint8)
            image[:, :, channel][mask] = np.broadcast_to(values, mask.shape)[mask]

    # Derive coastlines and continent boundaries directly from the ID map.
    # This stays clean at the ±180° seam and avoids polar polygon spikes.
    land_pixels = (ids >= 1) & (ids <= 7)
    edge = np.zeros_like(land_pixels)
    for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0)):
        neighbor_ids = np.roll(ids, (dy, dx), axis=(0, 1))
        neighbor_land = (neighbor_ids >= 1) & (neighbor_ids <= 7)
        edge |= (ids != neighbor_ids) & (land_pixels | neighbor_land)
    thick_edge = edge.copy()
    for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0)):
        thick_edge |= np.roll(edge, (dy, dx), axis=(0, 1))
    image[thick_edge] = np.array((232, 246, 249), dtype=np.uint8)
    return Image.fromarray(image, mode="RGB")


def save_masks(region_ids):
    ids = np.array(region_ids)
    for name, region_id in REGIONS.items():
        mask = np.where(ids == region_id, 255, 0).astype(np.uint8)
        safe_name = name.replace(" ", "_")
        Image.fromarray(mask, mode="L").save(MASKS / f"Mask_{region_id:02d}_{safe_name}_4K.png", optimize=True)


def make_preview(base, region_ids):
    preview_w, preview_h = 1600, 1060
    canvas = Image.new("RGB", (preview_w, preview_h), (6, 18, 38))
    resized = base.resize((1536, 768), Image.Resampling.LANCZOS)
    canvas.paste(resized, (32, 32))
    draw = ImageDraw.Draw(canvas)
    try:
        font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", 28)
        small = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf", 21)
    except OSError:
        font = small = ImageFont.load_default()
    draw.text((32, 820), "UNITY GLOBE REGION TEXTURE — 12 SELECTABLE REGIONS", fill=(235, 246, 255), font=font)
    entries = list(REGIONS.items())
    for i, (name, region_id) in enumerate(entries):
        col = i % 4
        row = i // 4
        x = 38 + col * 390
        y = 875 + row * 52
        color = CONTINENT_COLORS.get(region_id, OCEAN_COLORS.get(region_id, (255, 255, 255)))
        draw.rounded_rectangle((x, y, x + 34, y + 34), radius=6, fill=color, outline=(235, 246, 255), width=1)
        draw.text((x + 46, y + 5), f"{region_id:02d}  {name}", fill=(220, 235, 248), font=small)
    canvas.save(TEXTURES / "EducationalGlobe_TexturePreview.png", optimize=True)


def main():
    TEXTURES.mkdir(parents=True, exist_ok=True)
    MASKS.mkdir(parents=True, exist_ok=True)
    countries = json.loads(COUNTRIES.read_text())
    land = json.loads(LAND.read_text())
    region_ids = create_region_id_map(countries)
    base = create_base_texture(region_ids, land)
    base.save(TEXTURES / "EducationalGlobe_BaseColor_4K.png", optimize=True)
    region_ids.save(TEXTURES / "EducationalGlobe_RegionID_4K.png", optimize=True)
    save_masks(region_ids)
    make_preview(base, region_ids)
    print("Generated", WIDTH, "x", HEIGHT, "base, region map, and", len(REGIONS), "masks")


if __name__ == "__main__":
    main()
