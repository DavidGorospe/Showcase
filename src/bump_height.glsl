// Create a bumpy surface by using procedural noise to generate a height (
// displacement in normal direction).
//
// Inputs:
//   is_moon  whether we're looking at the moon or centre planet
//   s  3D position of seed for noise generation
// Returns elevation adjust along normal (values between -0.1 and 0.1 are
//   reasonable.
float bump_height( bool is_moon, vec3 s)
{
  float freq = 0.0;
  if (is_moon) {
    freq = 2.5;
  } else {
    freq = 3.0;
  }

  float n = 0.0;
  float amplitude = 1.0;
  float total_amp = 0.0;
  for (int i = 0; i < 4; i++) {
    n += amplitude * improved_perlin_noise(s * freq);
    total_amp += amplitude;
    freq *= 2.0;
    amplitude *= 0.5;
  }

  float max_bump = 0.05; 

  float min_bump = -max_bump;
  float bump = mix(min_bump, max_bump, 0.5 * n + 0.5); 

  return bump;
}
