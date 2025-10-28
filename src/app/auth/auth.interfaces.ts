/**
 * Define los datos que se ENVÍAN al backend para iniciar sesión.
 * Coincide con el DTO LoginDto de .NET.
 */
export interface LoginRequest {
  username: string;
  password: string;
}

/**
 * Define los datos que se ENVÍAN al backend para registrar un nuevo usuario.
 * Coincide con el DTO RegisterDto de .NET.
 */
export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

/**
 * Define la estructura de la respuesta que RECIBIMOS del backend tras un login o registro exitoso.
 * Coincide con el DTO NewUserDto de .NET.
 */
export interface LoginResponse {
  userName: string; 
  email: string;
  token: string;
}