using CursoMongo.API.Controllers.Outputs;
using CursoMongo.API.Data.Schemas;
using CursoMongo.API.Domain.Entities;
using CursoMongo.API.Domain.Enums;
using CursoMongo.API.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;

namespace CursoMongo.API.Data.Repositories;

public class RestauranteRepository
{
    private readonly IMongoCollection<RestauranteSchema> _restaurantes;
    private readonly IMongoCollection<AvaliacaoSchema> _avaliacoes;

    public RestauranteRepository(MongoContext context)
    {
        _restaurantes = context.Db.GetCollection<RestauranteSchema>("restaurantes");
        _avaliacoes = context.Db.GetCollection<AvaliacaoSchema>("avaliacoes");
    }

    public void Inserir(Restaurante restaurante)
    {
        var document = new RestauranteSchema
        {
            Nome = restaurante.Nome,
            Cozinha = restaurante.Cozinha,
            Endereco = new EnderecoSchema
            {
                Logradouro = restaurante.Endereco.Logradouro,
                Numero = restaurante.Endereco.Numero,
                Cidade = restaurante.Endereco.Cidade,
                Cep = restaurante.Endereco.Cep,
                UF = restaurante.Endereco.UF
            }
        };

        _restaurantes.InsertOne(document);
    }

    public async Task<IEnumerable<Restaurante>> ObterTodos()
    {
        List<Restaurante> restaurantes = new();

        await _restaurantes.AsQueryable().ForEachAsync(restauranteSchema =>
        {
            restaurantes.Add(restauranteSchema.ConverterParaDomain());
        });
        return restaurantes;
    }

    public Restaurante ObterPorId(string id)
    {
        var document = _restaurantes.AsQueryable()
            .FirstOrDefault(r => r.Id == id);

        if (document is null)
            return null;

        return document.ConverterParaDomain();
    }

    public bool AlterarCompleto(Restaurante restaurante)
    {
        var document = new RestauranteSchema
        {
            Id = restaurante.Id,
            Nome = restaurante.Nome,
            Cozinha = restaurante.Cozinha,
            Endereco = new EnderecoSchema
            {
                Logradouro = restaurante.Endereco.Logradouro,
                Numero = restaurante.Endereco.Numero,
                Cidade = restaurante.Endereco.Cidade,
                Cep = restaurante.Endereco.Cep,
                UF = restaurante.Endereco.UF
            }
        };

        var resultado = _restaurantes.ReplaceOne(_ => _.Id == document.Id, document);

        return resultado.ModifiedCount > 0;
    }

    public bool AlterarCozinha(string id, ECozinha cozinha)
    {
        var atualizacao = Builders<RestauranteSchema>.Update.Set(r => r.Cozinha, cozinha);

        var resultado = _restaurantes.UpdateOne(r => r.Id == id, atualizacao);

        return resultado.ModifiedCount > 0;
    }

    public IEnumerable<Restaurante> ObterPorNome(string nome)
    {
        var restaurantes = new List<Restaurante>();

        _restaurantes.AsQueryable()
            .Where(r => r.Nome.ToUpper().Contains(nome.ToUpper()))
            .ToList()
            .ForEach(r => restaurantes.Add(r.ConverterParaDomain()));

        return restaurantes;
    }

    public void Avaliar(string restauranteId, Avaliacao avaliacao)
    {
        var document = new AvaliacaoSchema
        {
            RestauranteId = restauranteId,
            Estrelas = avaliacao.Estrelas,
            Comentario = avaliacao.Comentario
        };

        _avaliacoes.InsertOne(document);
    }

    public async Task<Dictionary<Restaurante, double>> ObterTop3()
    {
        var retorno = new Dictionary<Restaurante, double>();

        var top3Restaurantes = _avaliacoes.Aggregate()
            .Group(_ => _.RestauranteId, g => new { RestauranteId = g.Key, MediaEstrelas = g.Average(a => a.Estrelas) })
            .SortByDescending(_ => _.MediaEstrelas)
            .Limit(3);

        await top3Restaurantes.ForEachAsync(_ =>
        {
            var restaurante = ObterPorId(_.RestauranteId);

            _avaliacoes.AsQueryable()
                .Where(a => a.RestauranteId == _.RestauranteId)
                .ToList()
                .ForEach(a => restaurante.AtribuirAvaliacao(a.ConverterParaDomain()));

            retorno.Add(restaurante, _.MediaEstrelas);
        });
        return retorno;
    }

    public (long, long) Remover(string restauranteId)
    {
        var resultadoRestaurantes = _restaurantes.DeleteMany(a => a.Id == restauranteId);
        var resultadoAvaliacoes = _avaliacoes.DeleteMany(a => a.RestauranteId == restauranteId);

        return (resultadoRestaurantes.DeletedCount, resultadoAvaliacoes.DeletedCount);
    }

    public async Task<IEnumerable<Restaurante>> ObterPorBuscaTextual(string texto)
    {
        var restaurantes = new List<Restaurante>();

        var filter = Builders<RestauranteSchema>.Filter.Text(texto);

        await _restaurantes
            .AsQueryable()
            .Where(_ => filter.Inject())
            .ForEachAsync(d => restaurantes.Add(d.ConverterParaDomain()));

        return restaurantes;
    }    
}
