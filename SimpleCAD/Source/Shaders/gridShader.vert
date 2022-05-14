#version 400
layout (location = 0) in vec4 position;
layout (location = 1) in vec4 color;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float falloff_factor;
uniform vec3 background;
out vec4 vs_color;

void main() {
	gl_Position = position * model * view * projection;
	float falloff = max(0, (falloff_factor - length(position)) / falloff_factor);
	vs_color = vec4(falloff * color.r + (1-falloff) * background.r, falloff * color.g + (1-falloff) * background.g, falloff * color.b + (1-falloff) * background.b, 1.0f);
}