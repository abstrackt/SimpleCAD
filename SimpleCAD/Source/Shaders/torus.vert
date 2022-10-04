#version 400
layout (location = 0) in vec4 position;
layout (location = 1) in vec4 color;
layout (location = 2) in vec2 uv;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec4 vs_color;
out vec2 vs_uv;

void main() {
	gl_Position = position * model * view * projection;
	vs_color = color;
	vs_uv = uv;
}