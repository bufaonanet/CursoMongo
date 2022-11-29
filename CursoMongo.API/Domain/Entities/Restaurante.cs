using CursoMongo.API.Domain.Enums;
using CursoMongo.API.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;

namespace CursoMongo.API.Domain.Entities;

public class Restaurante : AbstractValidator<Restaurante>
{
    public Restaurante(string nome, ECozinha cozinha)
    {
        Nome = nome;
        Cozinha = cozinha;
        Avaliacoes = new List<Avaliacao>();
    }

    public Restaurante(string id, string nome, ECozinha cozinha)
    {
        Id = id;
        Nome = nome;
        Cozinha = cozinha;
        Avaliacoes = new List<Avaliacao>();
    }

    public string Id { get; init; }
    public string Nome { get; init; }
    public ECozinha Cozinha { get; init; }
    public Endereco Endereco { get; private set; }
    public List<Avaliacao> Avaliacoes { get; private set; }

    public ValidationResult ValidationResult { get; set; }

    public bool Validar()
    {
        ValidarNome();
        ValidationResult = Validate(this);

        ValidarEndereco();

        return ValidationResult.IsValid;
    }

    public void AtribuirAvaliacao(Avaliacao avaliacao)
    {
        Avaliacoes.Add(avaliacao);
    }

    public void AtribuirEndereco(Endereco endereco)
    {
        Endereco = endereco;
    }

    private void ValidarNome()
    {
        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("Nome não pode ser vazio.")
            .MaximumLength(30).WithMessage("Nome pode ter no maximo 30 caracteres.");
    }

    private void ValidarEndereco()
    {
        if (Endereco.Validar())
            return;

        foreach (var erro in Endereco.ValidationResult.Errors)
        {
            ValidationResult.Errors.Add(erro);
        }
    }


}
