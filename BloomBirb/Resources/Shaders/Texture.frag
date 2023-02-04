//Specifying the version like in our vertex shader.
#version 330 core

in vec2 f_uv;
in vec4 f_col;

uniform sampler2D u_Texture0;

out vec4 FragColor;

void main()
{
    //Here we are setting our output variable, for which the name is not important.
    FragColor = texture(u_Texture0, f_uv) * f_col;
}
