shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;
uniform sampler2D noise : repeat_enable;

void fragment() {
	// Called for every pixel the material is visible on.
	float offset = (texture(noise, UV + TIME).r - 0.5) * 0.005;
	COLOR = texture(screen_texture, SCREEN_UV + offset); // uv or screen_uv
}
