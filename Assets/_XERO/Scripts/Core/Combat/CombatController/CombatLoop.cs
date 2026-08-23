using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public partial class CombatController : MoreMountains.Tools.MMSingleton<CombatController>
{
    private CancellationTokenSource cancellationTokenSource;

    private void RunCombatLoop()
    {
        if (cancellationTokenSource != null)
        {
            Debug.LogWarning("[CombatController] Combat loop is already running. Stopping the existing loop before starting a new one.");
            StopCombatLoop();
        }
        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        RunCombatLoopAsync(cancellationTokenSource.Token).Forget();
    }

    private void StopCombatLoop()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
    }

    private async UniTask RunCombatLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            turnSelector.NextTurn(alivePlayerParticipants, aliveEnemyParticipants);
            var currentParticipant = turnSelector.GetCurrentParticipant();
            
            if (currentParticipant == null)
            {
                Debug.LogError("[CombatController] Current participant is null.");
                break;
            }

            if (enableDebug)
            {
                Debug.Log($"<color=#55AAFF>[Combat]</color> Current turn: {turnSelector.GetCurrentParticipant().CombatantName}");
                string timeline = "Timeline: ";
                foreach (var participant in turnSelector.TurnTimeline)
                {
                    timeline += participant.CombatantName + " -> ";
                }
                Debug.Log($"<color=#55AAFF>[Combat]</color> {timeline}");
            }

            if (currentParticipant.TurnExec == null)
            {
                Debug.LogError($"[CombatController] {currentParticipant.CombatantName} has no turn participant assigned.");
                break;
            }
            
            // Create a linked CancellationTokenSource for this turn so the participant's
            // destroy token and the global combat token are both observed.
            // *without overwriting the global token, so that partcipant's destroy token is observed during his turn execution.
            using (var turnCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, currentParticipant.GetCancellationTokenOnDestroy()))
            {
                await currentParticipant.TurnExec.ExecuteTurn(currentParticipant, turnCancellationTokenSource.Token);
            }
            Debug.Log($"<color=#55AAFF>[Combat]</color> {currentParticipant.CombatantName} completed their turn.");
        }
    }
}
