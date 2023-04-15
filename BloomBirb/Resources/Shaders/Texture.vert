#version 430 core

#include "shared.h"

uniform mat3 u_projMatrix = mat3(1.0);

void main()
{
    gl_Position =  vec4((u_projMatrix * vec3(v_pos, 1)).xy, v_depth, 1);

    f_uv = (v_texRegionOrigin + (v_uv * v_texRegionSize))/u_textureSize;
    f_col = v_col;
}
