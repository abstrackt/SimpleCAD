#version 400
layout (location = 0) in vec4 position;
layout (location = 1) in vec4 color;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec4 vs_color;

void main() {
	gl_Position = position * model * view * projection;
	vs_color = color;
}