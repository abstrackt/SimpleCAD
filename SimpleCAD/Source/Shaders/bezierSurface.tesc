﻿#version 460
layout (vertices = 16) out;

uniform int tess_u;
uniform int tess_v;

in vec4 vs_color[];
out vec4 tesc_colors[];
flat out int tesc_primitives[];

void main() {
	gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;

    gl_TessLevelOuter[0] = tess_v;
    gl_TessLevelOuter[1] = tess_u;
	gl_TessLevelOuter[2] = tess_v;
	gl_TessLevelOuter[3] = tess_u;
	gl_TessLevelInner[0] = tess_u;
	gl_TessLevelInner[1] = tess_v;

	tesc_colors[gl_InvocationID] = vs_color[gl_InvocationID];
	tesc_primitives[gl_InvocationID] = gl_PrimitiveID;
}