using UnityEngine;

public class NPCInfo
{
    public string name;
    public NPCPersonality personality;

    public NPCInfo(string _name)
    {
        name = _name;
        personality = new NPCPersonality();
    }
}

//TODO: Struct?
public class NPCPersonality
{
    float openness;
    float consienciousness;
    float extraversion;
    float agreeableness;
    float neuroticism;

    public NPCPersonality()
    {
        openness = Random.Range(-1f, 1f);
        consienciousness = Random.Range(-1f, 1f);
        extraversion = Random.Range(-1f, 1f);
        agreeableness = Random.Range(-1f, 1f);
        neuroticism = Random.Range(-1f, 1f);
    }
}
