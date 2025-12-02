using Moq;
using Peluqueria.Application.Services;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Dtos.Account;
using Peluqueria.Application.Exceptions;
using Microsoft.AspNetCore.Identity;
using Peluqueria.Application.Dtos.Events;

namespace Peluqueria.Tests.Services
{
    /// <summary>
    /// Define las pruebas unitarias para validar la lógica de negocio, autenticación y registro en <see cref="AccountService"/>.
    /// </summary>
    public class AccountServiceTests
    {
        private readonly Mock<IIdentityService> _identityServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly AccountService _service;

        public AccountServiceTests()
        {
            _identityServiceMock = new Mock<IIdentityService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _messagePublisherMock = new Mock<IMessagePublisher>();

            _service = new AccountService(
                _identityServiceMock.Object,
                _tokenServiceMock.Object,
                _messagePublisherMock.Object
            );
        }

        /// <summary>
        /// Verifica que el método <see cref="AccountService.LoginAsync"/> lance una excepción de tipo <see cref="ReglaNegocioException"/>
        /// con el código de error correspondiente cuando el usuario no existe en el sistema.
        /// </summary>
        [Fact]
        public async Task LoginAsync_DebeLanzarExcepcion_CuandoUsuarioNoExiste()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "usuario_inexistente", Password = "password123" };
            _identityServiceMock.Setup(x => x.GetUserByUsernameAsync(It.IsAny<string>()))
                .ReturnsAsync((AppUserMinimalDto?)null);

            // Act & Assert
            var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.LoginAsync(loginDto));
            Assert.Equal(CodigoError.CREDENCIALES_INVALIDAS, excepcion.CodigoError);
        }

        /// <summary>
        /// Verifica que el método <see cref="AccountService.LoginAsync"/> lance una excepción de tipo <see cref="ReglaNegocioException"/>
        /// cuando el usuario existe pero la contraseña proporcionada es incorrecta.
        /// </summary>
        [Fact]
        public async Task LoginAsync_DebeLanzarExcepcion_CuandoPasswordEsIncorrecto()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "usuario_existente", Password = "password_erroneo" };
            var usuarioSimulado = new AppUserMinimalDto { UserName = "usuario_existente" };

            _identityServiceMock.Setup(x => x.GetUserByUsernameAsync("usuario_existente"))
                .ReturnsAsync(usuarioSimulado);

            _identityServiceMock.Setup(x => x.CheckPasswordSignInAsync(usuarioSimulado.UserName, loginDto.Password, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act & Assert
            var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.LoginAsync(loginDto));
            Assert.Equal(CodigoError.CREDENCIALES_INVALIDAS, excepcion.CodigoError);
        }

        /// <summary>
        /// Verifica que el método <see cref="AccountService.LoginAsync"/> retorne un DTO con el token de acceso generado
        /// cuando las credenciales (usuario y contraseña) son válidas.
        /// </summary>
        [Fact]
        public async Task LoginAsync_DebeRetornarToken_CuandoCredencialesSonValidas()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "usuario_valido", Password = "password_correcto" };
            var usuarioSimulado = new AppUserMinimalDto { UserName = "usuario_valido", Email = "test@test.com" };
            var rolesSimulados = new List<string> { "Cliente" };
            var tokenEsperado = "token_jwt_simulado";

            _identityServiceMock.Setup(x => x.GetUserByUsernameAsync("usuario_valido"))
                .ReturnsAsync(usuarioSimulado);

            _identityServiceMock.Setup(x => x.CheckPasswordSignInAsync(usuarioSimulado.UserName, loginDto.Password, false))
                .ReturnsAsync(SignInResult.Success);

            _identityServiceMock.Setup(x => x.GetRolesAsync(usuarioSimulado.UserName))
                .ReturnsAsync(rolesSimulados);

            _tokenServiceMock.Setup(x => x.CreateToken(usuarioSimulado, rolesSimulados))
                .Returns(tokenEsperado);

            // Act
            var resultado = await _service.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(usuarioSimulado.UserName, resultado.UserName);
            Assert.Equal(tokenEsperado, resultado.Token);
        }

        /// <summary>
        /// Verifica que el método <see cref="AccountService.RegisterAsync"/> lance una excepción de tipo <see cref="EntidadYaExisteException"/>
        /// cuando el servicio de identidad reporta duplicidad en el nombre de usuario o correo electrónico.
        /// </summary>
        [Fact]
        public async Task RegisterAsync_DebeLanzarExcepcion_CuandoUsuarioOCorreoSonDuplicados()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "duplicado" };
            var errorIdentidad = new IdentityError { Code = "DuplicateUserName", Description = "El usuario ya existe" };

            _identityServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(errorIdentidad));

            // Act & Assert
            await Assert.ThrowsAsync<EntidadYaExisteException>(() => _service.RegisterAsync(registerDto));
        }

        /// <summary>
        /// Verifica que el método <see cref="AccountService.RegisterAsync"/> lance una excepción de tipo <see cref="ReglaNegocioException"/>
        /// cuando ocurre un error genérico durante la creación del usuario en el sistema de identidad.
        /// </summary>
        [Fact]
        public async Task RegisterAsync_DebeLanzarExcepcion_CuandoFallaCreacionPorErrorGenerico()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "error_gen" };
            var errorIdentidad = new IdentityError { Code = "PasswordTooShort", Description = "La contraseña es muy corta" };

            _identityServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(errorIdentidad));

            // Act & Assert
            var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.RegisterAsync(registerDto));
            Assert.Equal(CodigoError.ERROR_GENERICO, excepcion.CodigoError);
        }

        /// <summary>
        /// Verifica que el método <see cref="AccountService.RegisterAsync"/> lance una excepción de tipo <see cref="ReglaNegocioException"/>
        /// si el usuario se crea correctamente pero falla la asignación del rol.
        /// </summary>
        [Fact]
        public async Task RegisterAsync_DebeLanzarExcepcion_CuandoFallaAsignacionDeRol()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "usuario_sin_rol" };

            _identityServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _identityServiceMock.Setup(x => x.AddUserToRoleAsync(It.IsAny<string>(), "Cliente"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error al asignar rol" }));

            // Act & Assert
            await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.RegisterAsync(registerDto));
        }

        /// <summary>
        /// Verifica que el método <see cref="AccountService.RegisterAsync"/> complete el registro exitosamente y publique el evento de integración
        /// correspondiente a RabbitMQ cuando todos los pasos son correctos.
        /// </summary>
        [Fact]
        public async Task RegisterAsync_DebeRegistrarYPublicarEvento_CuandoDatosSonValidos()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "nuevo_cliente",
                Email = "cliente@test.com",
                NombreCompleto = "Cliente Test",
                Telefono = "3001234567",
                Password = "Password123!"
            };
            var usuarioCreado = new AppUserMinimalDto { Id = "guid-nuevo", UserName = "nuevo_cliente", Email = "cliente@test.com" };

            _identityServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _identityServiceMock.Setup(x => x.AddUserToRoleAsync(It.IsAny<string>(), "Cliente"))
                .ReturnsAsync(IdentityResult.Success);

            _identityServiceMock.Setup(x => x.FindByNameAsync(registerDto.Username))
                .ReturnsAsync(usuarioCreado);

            // Act
            var resultado = await _service.RegisterAsync(registerDto);

            // Assert
            Assert.True(resultado.Succeeded);

            _messagePublisherMock.Verify(x => x.PublishAsync(
                It.Is<ClienteRegistradoEventDto>(e =>
                    e.IdentityId == "guid-nuevo" &&
                    e.Username == "nuevo_cliente" &&
                    e.Email == "cliente@test.com"),
                "cliente.registrado",
                "cliente_exchange"),
                Times.Once);
        }

        /// <summary>
        /// Verifica que el método <see cref="AccountService.GetNewUserDto"/> retorne null
        /// si el usuario solicitado no existe.
        /// </summary>
        [Fact]
        public async Task GetNewUserDto_DebeRetornarNull_CuandoUsuarioNoExiste()
        {
            // Arrange
            _identityServiceMock.Setup(x => x.FindByNameAsync("inexistente"))
                .ReturnsAsync((AppUserMinimalDto?)null);

            // Act
            var resultado = await _service.GetNewUserDto("inexistente", "cualquier_pass");

            // Assert
            Assert.Null(resultado);
        }

        /// <summary>
        /// Verifica que el método <see cref="AccountService.GetNewUserDto"/> retorne el DTO con el token
        /// si el usuario existe.
        /// </summary>
        [Fact]
        public async Task GetNewUserDto_DebeRetornarUsuarioConToken_CuandoExiste()
        {
            // Arrange
            var usuario = new AppUserMinimalDto { UserName = "existente", Email = "test@test.com" };
            var roles = new List<string> { "Admin" };

            _identityServiceMock.Setup(x => x.FindByNameAsync("existente")).ReturnsAsync(usuario);
            _identityServiceMock.Setup(x => x.GetRolesAsync("existente")).ReturnsAsync(roles);
            _tokenServiceMock.Setup(x => x.CreateToken(usuario, roles)).Returns("jwt_token");

            // Act
            var resultado = await _service.GetNewUserDto("existente", "pass");

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("jwt_token", resultado.Token);
        }

        /// <summary>
        /// Verifica que el método <see cref="AccountService.GetAllClientesAsync"/> retorne la lista de usuarios
        /// que poseen el rol de "Cliente".
        /// </summary>
        [Fact]
        public async Task GetAllClientesAsync_DebeRetornarListaDeClientes()
        {
            // Arrange
            var listaClientes = new List<ClienteResponseDto>
            {
                new ClienteResponseDto { Id = "1", Username = "c1" },
                new ClienteResponseDto { Id = "2", Username = "c2" }
            };

            _identityServiceMock.Setup(x => x.GetUsersByRoleAsync("Cliente"))
                .ReturnsAsync(listaClientes);

            // Act
            var resultado = await _service.GetAllClientesAsync();

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count());
        }
    }
}