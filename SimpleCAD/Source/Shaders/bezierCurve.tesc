#version 400
layout (vertices = 4) out;

uniform mat4 view;
uniform mat4 projection;

in vec4 vs_color[];
out vec4 tesc_colors[];

void main() {
	vec4 ss1 = gl_in[0].gl_Position * view * projection;
	vec4 ss2 = gl_in[1].gl_Position * view * projection;
	vec4 ss3 = gl_in[2].gl_Position * view * projection;
	vec4 ss4 = gl_in[3].gl_Position * view * projection;
    ss1 /= ss1.w;
	ss2 /= ss2.w;
	ss3 /= ss3.w;
	ss4 /= ss4.w;

	float tess_level = clamp(20 * (length(ss2.xy - ss1.xy) + length(ss3.xy - ss2.xy) + length(ss4.xy - ss3.xy)), 10, 64);

    tesc_colors[gl_InvocationID] = vs_color[gl_InvocationID];

	gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;

    gl_TessLevelOuter[0] = 1.0;
    gl_TessLevelOuter[1] = tess_level;
}