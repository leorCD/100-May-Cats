shader_type spatial;
render_mode blend_add, depth_draw_never, cull_disabled, unshaded;

uniform vec4 streak_color : source_color = vec4(1.0, 1.0, 1.0, 0.2);

void vertex() {
    // VERTEX.x is along the length of the quad (-0.5 to 0.5)
    // displace in Y (or Z) with a sine wave
    float wave = sin(VERTEX.x * 10.0 + TIME * 4.0) * 0.2;
    VERTEX.y += wave;
}

void fragment() {
    float taper = sin(UV.x * 0.5*PI);
    
    ALBEDO = streak_color.rgb;
    ALPHA = streak_color.a * taper * COLOR.a;
}