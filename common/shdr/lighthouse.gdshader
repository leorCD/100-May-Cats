shader_type spatial;

render_mode unshaded, blend_add, cull_disabled;

void fragment()
{
    float fade = UV.y;

    fade = pow(1.0 - fade, 10.0);

    ALBEDO = vec3(1.0, 0.95, 0.8);
    EMISSION = vec3(6.0);

    ALPHA = fade * 0.5;
}