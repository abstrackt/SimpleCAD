#version 400
layout( quads, equal_spacing, cw ) in;

uniform mat4 view;
uniform mat4 projection;

in vec4 tesc_colors[];
in vec2 tesc_uvs[];
out vec4 tese_color;
out vec2 tese_uv;

vec4 deboor(float t, vec4 p10, vec4 p20, vec4 p30, vec4 p40) {
	t += 2;

	float a21 = t / 3;
	float a31 = (t - 1) / 3;
    float a41 = (t - 2) / 3;

	float a32 = (t - 1) / 2;
    float a42 = (t - 2) / 2;

	float a43 = (t - 2) / 1;

    vec4 p41 = a41 * p40 + (1 - a41) * p30;
    vec4 p31 = a31 * p30 + (1 - a31) * p20;
    vec4 p21 = a21 * p20 + (1 - a21) * p10;

    vec4 p42 = a42 * p41 + (1 - a42) * p31;
    vec4 p32 = a32 * p31 + (1 - a32) * p21;

    vec4 p43 = a43 * p42 + (1 - a43) * p32;

    return p43;
}

void main() {
	vec4 p00 = gl_in[ 0 ].gl_Position;
	vec4 p10 = gl_in[ 1 ].gl_Position;
	vec4 p20 = gl_in[ 2 ].gl_Position;
	vec4 p30 = gl_in[ 3 ].gl_Position;
	vec4 p01 = gl_in[ 4 ].gl_Position;
	vec4 p11 = gl_in[ 5 ].gl_Position;
	vec4 p21 = gl_in[ 6 ].gl_Position;
	vec4 p31 = gl_in[ 7 ].gl_Position;
	vec4 p02 = gl_in[ 8 ].gl_Position;
	vec4 p12 = gl_in[ 9 ].gl_Position;
	vec4 p22 = gl_in[ 10 ].gl_Position;
	vec4 p32 = gl_in[ 11 ].gl_Position;
	vec4 p03 = gl_in[ 12 ].gl_Position;
	vec4 p13 = gl_in[ 13 ].gl_Position;
	vec4 p23 = gl_in[ 14 ].gl_Position;
	vec4 p33 = gl_in[ 15 ].gl_Position;

	vec2 uv00 = tesc_uvs[0];
	vec2 uv10 = tesc_uvs[1];
	vec2 uv20 = tesc_uvs[2];
	vec2 uv30 = tesc_uvs[3];
	vec2 uv01 = tesc_uvs[4];
	vec2 uv11 = tesc_uvs[5];
	vec2 uv21 = tesc_uvs[6];
	vec2 uv31 = tesc_uvs[7];
	vec2 uv02 = tesc_uvs[8];
	vec2 uv12 = tesc_uvs[9];
	vec2 uv22 = tesc_uvs[10];
	vec2 uv32 = tesc_uvs[11];
	vec2 uv03 = tesc_uvs[12];
	vec2 uv13 = tesc_uvs[13];
	vec2 uv23 = tesc_uvs[14];
	vec2 uv33 = tesc_uvs[15];

	float u = gl_TessCoord.x;
	float v = gl_TessCoord.y;

	vec4 p0 = deboor(v, p00, p01, p02, p03);
	vec4 p1 = deboor(v, p10, p11, p12, p13);
	vec4 p2 = deboor(v, p20, p21, p22, p23);
	vec4 p3 = deboor(v, p30, p31, p32, p33);

	vec4 p = deboor(u, p0, p1, p2, p3);

	float bu0 = (1.-u) * (1.-u) * (1.-u);
	float bu1 = 3. * u * (1.-u) * (1.-u);
	float bu2 = 3. * u * u * (1.-u);
	float bu3 = u * u * u;

	float bv0 = (1.-v) * (1.-v) * (1.-v);
	float bv1 = 3. * v * (1.-v) * (1.-v);
	float bv2 = 3. * v * v * (1.-v);
	float bv3 = v * v * v;

	vec2 uv = bu0 * ( bv0*uv00 + bv1*uv01 + bv2*uv02 + bv3*uv03 ) 
	+ bu1 * ( bv0*uv10 + bv1*uv11 + bv2*uv12 + bv3*uv13 )
	+ bu2 * ( bv0*uv20 + bv1*uv21 + bv2*uv22 + bv3*uv23 )
	+ bu3 * ( bv0*uv30 + bv1*uv31 + bv2*uv32 + bv3*uv33 );

	gl_Position = p * view * projection;

	tese_color = tesc_colors[0];
	tese_uv = uv;
}