// Filter an input value to perform a smooth step. This function should be a
// cubic polynomial with smooth_step(0) = 0, smooth_step(1) = 1, and zero first
// derivatives at f=0 and f=1. 
//
// Inputs:
//   f  input value
// Returns filtered output value
float smooth_step( float f)
{
  f = clamp(f, 0.0, 1.0);
  return f * f * (3.0 - 2.0 * f); // 3x^2 - 2x^3 
}

vec3 smooth_step( vec3 f)
{
  f = clamp(f, vec3(0.0), vec3(1.0));
  return f * f * (3.0 - 2.0 * f);
}
