#version 330 core

#include "shared.h"

void main()
{
    gl_Position =  u_TransMat * vec4(v_pos, 1);

    f_uv = v_uv;
}
