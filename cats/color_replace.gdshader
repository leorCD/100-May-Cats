shader_type spatial;
render_mode cull_disabled, diffuse_burley, specular_schlick_ggx;  // add this line

uniform vec4 target_color : source_color = vec4(1.0, 0.0, 1.0, 1.0);
uniform vec4 replace_color : source_color = vec4(0.0, 1.0, 0.0, 1.0);
uniform float threshold : hint_range(0.0, 1.0) = 0.1;
uniform sampler2D texture_albedo : source_color, filter_linear_mipmap;

void fragment() {
    vec4 tex = texture(texture_albedo, UV);
    float diff = distance(tex.rgb, target_color.rgb);
    ALBEDO = diff < threshold ? replace_color.rgb : tex.rgb;
    ALPHA = tex.a;
}