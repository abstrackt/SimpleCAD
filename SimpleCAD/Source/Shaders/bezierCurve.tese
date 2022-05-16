#version 400
layout (isolines) in;

uniform mat4 view;
uniform mat4 projection;

in vec4 tesc_colors[];
out vec4 tese_color;

void main() {
    float u = gl_TessCoord.x;
	float v = gl_TessCoord.y;

    float B0 = (1.-u)*(1.-u)*(1.-u);
    float B1 = 3.*u*(1.-u)*(1.-u);
    float B2 = 3.*u*u*(1.-u);
    float B3 = u*u*u;
 
    vec4 p = vec4(
    B0*gl_in[0].gl_Position.xyz + 
    B1*gl_in[1].gl_Position.xyz + 
    B2*gl_in[2].gl_Position.xyz + 
    B3*gl_in[3].gl_Position.xyz, 1);

    gl_Position = p * view * projection;

    tese_color = tesc_colors[0];
}