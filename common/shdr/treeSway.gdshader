shader_type spatial;

uniform sampler2D noise_texture : hint_default_white;
uniform float sway_strength : hint_range(0.0, 1.0) = 0.08;
uniform float sway_speed : hint_range(0.0, 5.0) = 0.8;
uniform float noise_scale : hint_range(0.0, 2.0) = 0.15;
uniform float tree_height : hint_range(0.1, 20.0) = 5.0;

void vertex() {
    // World XZ position for per-tree phase variation
    vec2 world_xz = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xz;

    // UV shifts over time — two axes at different speeds for organic feel
    vec2 noise_uv = world_xz * noise_scale + vec2(TIME * sway_speed * 0.3, TIME * sway_speed * 0.1);

    // Sample noise, remap 0..1 → -1..1
    float noise = texture(noise_texture, noise_uv).r * 2.0 - 1.0;

    // Height mask — base stays rigid, canopy moves most
    float height_factor = pow(max(VERTEX.y / tree_height, 0.0), 2.0);

    // Apply displacement
    VERTEX.x += noise * sway_strength * height_factor;
    VERTEX.z += noise * sway_strength * 0.3 * height_factor;
}