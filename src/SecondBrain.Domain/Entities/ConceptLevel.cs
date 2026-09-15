namespace SecondBrain.Domain.Entities;

// Nivel de dificuldade de um Concept, pra nao misturar basico com avancado nas
// listagens - quem esta aprendendo uma linguagem do zero segue essa ordem.
// Nullable no Concept: verbetes criados a mao pela pessoa nao precisam classificar nivel.
public enum ConceptLevel
{
    Basico,
    Intermediario,
    Avancado,
}
