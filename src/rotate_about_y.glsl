// Inputs:
//   theta  amount y which to rotate (in radians)
// Return a 4x4 matrix that rotates a given 3D point/vector about the y-axis by
// the given amount.
mat4 rotate_about_y(float theta)
{
  float c = cos(theta);
  float s = sin(theta);
  mat4 M = identity();
  M[0][0] =  c;  M[0][2] =  s;
  M[2][0] = -s;  M[2][2] =  c;
  return M;
}

