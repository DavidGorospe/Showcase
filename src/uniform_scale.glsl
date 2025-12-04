// Inputs:
//   s  amount to scale in all directions
// Return a 4x4 matrix that scales and input 3D position/vector by s in all 3
// directions.
mat4 uniform_scale(float s)
{
  mat4 M = identity();
  M[0][0] = s;
  M[1][1] = s;
  M[2][2] = s;
  return M;
}

