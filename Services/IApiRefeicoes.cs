using RefeicoesApp.Models;
using Refit;

namespace RefeicoesApp.Services;

public interface IApiRefeicoes
{
    [Multipart]
    [Post("/api/identificacao/registrar")]
    Task<ApiResponse<IdentificacaoResult>> IdentificarEFinalizar([AliasAs("foto")] StreamPart stream);
}