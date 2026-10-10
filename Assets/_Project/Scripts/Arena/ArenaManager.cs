using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    [Header("Combatentes")]
    public Combatant playerCombatant;
    public Combatant enemyCombatant;

    [Header("Tamer")]
    public Transform playerTamer;

    [Header("Spawns")]
    public Transform playerMonSpawn;
    public Transform enemyMonSpawn;
    public Transform playerTamerSpawn;
    public Transform collectibleSpawn;

    [Header("Configuração")]
    public bool positionCombatantsOnStart = true;
    public bool positionTamerOnStart = true;

    private void Start()
    {
        if (positionCombatantsOnStart)
        {
            PositionInitialCombatants();
        }

        if (positionTamerOnStart)
        {
            PositionInitialTamer();
        }
    }

    public void PositionInitialCombatants()
    {
        MoveTransformToSpawn(playerCombatant != null ? playerCombatant.transform : null, playerMonSpawn);
        MoveTransformToSpawn(enemyCombatant != null ? enemyCombatant.transform : null, enemyMonSpawn);
    }

    public void PositionInitialTamer()
    {
        MoveTransformToSpawn(playerTamer, playerTamerSpawn);
    }

    private void MoveTransformToSpawn(Transform target, Transform spawn)
    {
        if (target == null || spawn == null) return;

        Vector3 currentPosition = target.position;

        target.position = new Vector3(
            spawn.position.x,
            spawn.position.y,
            currentPosition.z
        );
    }
}