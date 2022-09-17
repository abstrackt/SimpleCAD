#version 400
layout( quads, equal_spacing, ccw ) in;

uniform mat4 view;
uniform mat4 projection;

in vec4 tesc_colors[];
out vec4 tese_color;

vec4 casteljau(float t, vec4 p10, vec4 p20, vec4 p30, vec4 p40) {
    vec4 p11 = (1 - t) * p10 + t * p20;
    vec4 p21 = (1 - t) * p20 + t * p30;
    vec4 p31 = (1 - t) * p30 + t * p40;

    vec4 p12 = (1 - t) * p11 + t * p21;
    vec4 p22 = (1 - t) * p21 + t * p31;

    return (1 - t) * p12 + t * p22;
}

void main() {
    vec4 p0 = gl_in[0].gl_Position;
    vec4 p1 = gl_in[1].gl_Position;
    vec4 p2 = gl_in[2].gl_Position;
    vec4 p3 = gl_in[3].gl_Position;     
    vec4 p4 = gl_in[4].gl_Position;
    vec4 p5 = gl_in[5].gl_Position;
    vec4 p6 = gl_in[6].gl_Position;
    vec4 p7 = gl_in[7].gl_Position;
    vec4 p8 = gl_in[8].gl_Position;
    vec4 p9 = gl_in[9].gl_Position;
    vec4 p10 = gl_in[10].gl_Position;
    vec4 p11 = gl_in[11].gl_Position;
    vec4 p12 = gl_in[12].gl_Position;
    vec4 p13 = gl_in[13].gl_Position;
    vec4 p14 = gl_in[14].gl_Position;
    vec4 p15 = gl_in[15].gl_Position; 
    vec4 p16 = gl_in[16].gl_Position;
    vec4 p17 = gl_in[17].gl_Position;
    vec4 p18 = gl_in[18].gl_Position;
    vec4 p19 = gl_in[19].gl_Position;

    float u = gl_TessCoord.x;
    float v = gl_TessCoord.y;

    vec4 p12p11 = (u * p12 + v * p11) / max((u + v), 0.05);
    vec4 p14p13 = ((1 - u) * p14 + v * p13) / max((1 - u + v), 0.05);
    vec4 p7p8 = ((1 - u) * p7 + (1 - v) * p8) / max((2 - u - v), 0.05);
    vec4 p5p6 = (u * p5 + (1 - v) * p6) / max((1 + u - v), 0.05);

    vec4 r0 = casteljau(u, p0, p1, p2, p3);
    vec4 r1 = casteljau(u, p4, p5p6, p7p8, p9);
    vec4 r2 = casteljau(u, p10, p12p11, p14p13, p15);
    vec4 r3 = casteljau(u, p16, p17, p18, p19);

    vec4 p = casteljau(v, r0, r1, r2, r3);

    gl_Position = p * view * projection;

    tese_color = tesc_colors[0];
}