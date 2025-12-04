// Filter an input value to perform an even smoother step. This function should
// be a quintic polynomial with improved_smooth_step(0) = 0,
// improved_smooth_step(1) = 1, and zero first _and_ second derivatives at f=0
// and f=1. "Improving Noise" [Perlin 2002].
//
// Inputs:
//   f  input value
// Returns filtered output value
float improved_smooth_step( float f)
{
  f = clamp(f, 0.0, 1.0);
  return f * f * f * (f * (f * 6.0 - 15.0) + 10.0); // 6x^5 - 15x^4 + 10x^3
}
vec3 improved_smooth_step( vec3 f)
{
  f = clamp(f, vec3(0.0), vec3(1.0));
  return f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
}
