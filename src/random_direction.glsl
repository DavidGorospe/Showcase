// Generate a pseudorandom unit 3D vector
// 
// Inputs:
//   seed  3D seed
// Returns psuedorandom, unit 3D vector drawn from uniform distribution over
// the unit sphere (assuming random2 is uniform over [0,1]²).
//
// expects: random2.glsl, PI.glsl
vec3 random_direction( vec3 seed)
{
  // Get two uniform random numbers in [0, 1]
  vec2 r = random2(seed);

  // Map to spherical coordinates
  float theta = 2.0 * M_PI * r.x;   
  float z = 2.0 * r.y - 1.0;      
  float r_xy = sqrt(max(0.0, 1.0 - z * z)); 

  // Convert to Cartesian coordinates
  vec3 dir = vec3(
    r_xy * cos(theta),
    r_xy * sin(theta),
    z
  );

  return dir;
}
