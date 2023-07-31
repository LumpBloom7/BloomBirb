//Specifying the version like in our vertex shader.
#version 430 core

in FragData
{
    highp vec2 f_uv;
    lowp vec4 f_col;
};

uniform lowp sampler2D u_Texture0;

out lowp vec4 FragColor;

void main()
{
    lowp vec4 col = texture(u_Texture0, f_uv) * f_col;

    //Here we are setting our output variable, for which the name is not important.
    FragColor = col;
}
