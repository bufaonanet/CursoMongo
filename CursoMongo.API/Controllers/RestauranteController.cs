using CursoMongo.API.Controllers.Inputs;
using CursoMongo.API.Controllers.Outputs;
using CursoMongo.API.Data.Repositories;
using CursoMongo.API.Domain.Entities;
using CursoMongo.API.Domain.Enums;
using CursoMongo.API.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace CursoMongo.API.Controllers;

[ApiController]
public class RestauranteController : ControllerBase
{
    private readonly RestauranteRepository _respository;

    public RestauranteController(RestauranteRepository respository)
    {
        _respository = respository;
    }

    [HttpPost("restaurante")]
    public ActionResult IncluirRestaurante(RestauranteInclusao restauranteInclusao)
    {
        try
        {
            var cozinha = ECozinhaHelper
                        .ConverteDeInteiroParaEnum(restauranteInclusao.Cozinha);


            var restaurante = new Restaurante(restauranteInclusao.Nome, cozinha);
            var endereco = new Endereco(
                restauranteInclusao.Logradouro,
                restauranteInclusao.Numero,
                restauranteInclusao.Cidade,
                restauranteInclusao.UF,
                restauranteInclusao.Cep);

            restaurante.AtribuirEndereco(endereco);

            if (!restaurante.Validar())
            {
                return BadRequest(new
                {
                    erros = restaurante.ValidationResult
                        .Errors.Select(e => e.ErrorMessage),
                });
            };

            _respository.Inserir(restaurante);

            return Ok(new { data = "Restaurante inserido com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                erros = ex.Message
            });
        }
    }

    [HttpGet("restaurante/todos")]
    public async Task<ActionResult> ObterTodos()
    {
        var restaurantes = await _respository.ObterTodos();

        var listagem = restaurantes.Select(r => new RestauranteListagem
        {
            Id = r.Id,
            Nome = r.Nome,
            Cozinha = (int)r.Cozinha,
            Cidade = r.Endereco.Cidade
        });

        return Ok(new { data = listagem });
    }

    [HttpGet("restaurante/{id}")]
    public ActionResult ObterRestaurante(string id)
    {
        var restaurante = _respository.ObterPorId(id);

        if (restaurante == null)
            return NotFound();

        var exibicao = new RestauranteExibicao
        {
            Id = restaurante.Id,
            Nome = restaurante.Nome,
            Cozinha = (int)restaurante.Cozinha,
            Endereco = new EnderecoExibicao
            {
                Logradouro = restaurante.Endereco.Logradouro,
                Numero = restaurante.Endereco.Numero,
                Cidade = restaurante.Endereco.Cidade,
                Cep = restaurante.Endereco.Cep,
                UF = restaurante.Endereco.UF
            }
        };

        return Ok(new { data = exibicao });
    }

    [HttpPut("restaurante")]
    public ActionResult AlterarRestaurante([FromBody] RestauranteAlteracaoCompleta restauranteAlteracaoCompleta)
    {
        var restaurante = _respository.ObterPorId(restauranteAlteracaoCompleta.Id);

        if (restaurante == null)
            return NotFound();

        var cozinha = ECozinhaHelper.ConverteDeInteiroParaEnum(restauranteAlteracaoCompleta.Cozinha);
        restaurante = new Restaurante(restauranteAlteracaoCompleta.Id, restauranteAlteracaoCompleta.Nome, cozinha);
        var endereco = new Endereco(
            restauranteAlteracaoCompleta.Logradouro,
            restauranteAlteracaoCompleta.Numero,
            restauranteAlteracaoCompleta.Cidade,
            restauranteAlteracaoCompleta.UF,
            restauranteAlteracaoCompleta.Cep);

        restaurante.AtribuirEndereco(endereco);

        if (!restaurante.Validar())
        {
            return BadRequest(
                new
                {
                    errors = restaurante.ValidationResult.Errors.Select(_ => _.ErrorMessage)
                });
        }

        if (!_respository.AlterarCompleto(restaurante))
        {
            return BadRequest(new { errors = "Nenhum documento foi alterado" });
        }

        return Ok(new { data = "Restaurante alterado com sucesso" });
    }

    [HttpPatch("restaurante/{id}")]
    public ActionResult AlterarCozinha(string id, [FromBody] RestauranteAlteracaoParcial restauranteAlteracaoParcial)
    {
        var restaurante = _respository.ObterPorId(id);

        if (restaurante == null)
            return NotFound();

        var cozinha = ECozinhaHelper.ConverteDeInteiroParaEnum(restauranteAlteracaoParcial.Cozinha);

        if (!_respository.AlterarCozinha(id, cozinha))
        {
            return BadRequest(new { errors = "Nenhum documento foi alterado" });
        }

        return Ok(new { data = "Restaurante alterado com sucesso" });
    }

    [HttpGet("restaurante")]
    public ActionResult ObterRestaurantePorNome([FromQuery] string nome)
    {
        var restaurantes = _respository.ObterPorNome(nome);

        var listagem = restaurantes.Select(r => new RestauranteListagem
        {
            Id = r.Id,
            Nome = r.Nome,
            Cozinha = (int)r.Cozinha,
            Cidade = r.Endereco.Cidade
        });

        return Ok(new { data = listagem });
    }

    [HttpPatch("restaurante/{id}/avaliar")]
    public ActionResult AvaliarRestaurante(string id, [FromBody] AvaliacaoInclusao avaliacaoInclusao)
    {
        var restaurante = _respository.ObterPorId(id);

        if (restaurante == null)
            return NotFound();

        var avaliacao = new Avaliacao(avaliacaoInclusao.Estrelas, avaliacaoInclusao.Comentario);

        if (!avaliacao.Validar())
        {
            return BadRequest(
                new
                {
                    errors = avaliacao.ValidationResult.Errors.Select(_ => _.ErrorMessage)
                });
        }

        _respository.Avaliar(id, avaliacao);

        return Ok(new { data = "Restaurante avaliado com sucesso" });
    }

    [HttpGet("restaurante/top3")]
    public async Task<ActionResult> ObterTop3Restaurantes()
    {
        var top3 = await _respository.ObterTop3();

        var listagem = top3.Select(_ => new RestauranteTop3
        {
            Id = _.Key.Id,
            Nome = _.Key.Nome,
            Cozinha = (int)_.Key.Cozinha,
            Cidade = _.Key.Endereco.Cidade,
            Estrelas = _.Value
        });

        return Ok(new { data = listagem });
    }

    [HttpDelete("restaurante/{id}")]
    public ActionResult Remover(string id)
    {
        var restaurante = _respository.ObterPorId(id);

        if (restaurante == null)
            return NotFound();

        (var totalRestauranteRemovido, var totalAvaliacoesRemovidas) = _respository.Remover(id);

        return Ok(
            new
            {
                data = $"Total de exclusões: {totalRestauranteRemovido} restaurante com {totalAvaliacoesRemovidas} avaliações"
            }
        );
    }

    [HttpGet("restaurante/textual")]
    public async Task<ActionResult> ObterRestaurantePorBuscaTextual([FromQuery] string texto)
    {
        var restaurantes = await _respository.ObterPorBuscaTextual(texto);

        var listagem = restaurantes.ToList().Select(_ => new RestauranteListagem
        {
            Id = _.Id,
            Nome = _.Nome,
            Cozinha = (int)_.Cozinha,
            Cidade = _.Endereco.Cidade
        });

        return Ok(new { data = listagem });
    }


}