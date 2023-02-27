#version 430 core

#include "shared.h"

void main()
{
    gl_Position =  vec4(v_pos, v_depth, 1);

    f_uv = (v_texRegionOrigin + (v_uv * v_texRegionSize))/u_textureSize;
    f_col = v_col;
}
