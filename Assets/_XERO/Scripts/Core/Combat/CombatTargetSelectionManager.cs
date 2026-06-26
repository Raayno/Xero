// using System;
// using UnityEngine;

// public class CombatTargetSelectionManager : MonoBehaviour
// {
//     [Header("Selection")]
//     [SerializeField] private Camera selectionCamera;
//     [SerializeField] private LayerMask selectableLayerMask = ~0;
//     [SerializeField] private float maxRayDistance = 500f;

//     [Header("References")]
//     [SerializeField] private TargetSelector combatTargetProvider;

//     private Participant currentAttacker;
//     private AttackDataSO currentAttackData;

//     public bool IsSelectingTarget { get; private set; }

//     public event Action<Participant> TargetSelected;
//     public event Action TargetSelectionCancelled;

//     private void Awake()
//     {
//         if (selectionCamera == null)
//         {
//             selectionCamera = Camera.main;
//         }
//     }

//     private void Update()
//     {
//         if (!IsSelectingTarget)
//         {
//             return;
//         }

//         if (Input.GetMouseButtonDown(0))
//         {
//             TrySelectTargetFromMouse();
//             return;
//         }

//         if (Input.GetKeyDown(KeyCode.Escape))
//         {
//             CancelSelection();
//         }
//     }

//     public void BeginSelection(Participant attacker, AttackDataSO attackData)
//     {
//         if (attacker == null)
//         {
//             Debug.LogError("[CombatTargetSelectionManager] Cannot begin selection because attacker is null.");
//             return;
//         }

//         if (attackData == null)
//         {
//             Debug.LogError("[CombatTargetSelectionManager] Cannot begin selection because attack data is null.");
//             return;
//         }

//         if (combatTargetProvider == null)
//         {
//             Debug.LogError("[CombatTargetSelectionManager] CombatTargetProvider is not assigned.");
//             return;
//         }

//         currentAttacker = attacker;
//         currentAttackData = attackData;
//         IsSelectingTarget = true;

//         Debug.Log(
//             $"<color=#FFDD55>[Target Selection]</color> Select target for {attackData.name}.");
//     }

//     public void CancelSelection()
//     {
//         if (!IsSelectingTarget)
//         {
//             return;
//         }

//         ClearSelection();

//         Debug.Log("<color=#FFAA55>[Target Selection]</color> Selection cancelled.");

//         TargetSelectionCancelled?.Invoke();
//     }

//     private void TrySelectTargetFromMouse()
//     {
//         if (selectionCamera == null)
//         {
//             Debug.LogError("[CombatTargetSelectionManager] No selection camera assigned.");
//             return;
//         }

//         Ray ray = selectionCamera.ScreenPointToRay(Input.mousePosition);

//         if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, selectableLayerMask))
//         {
//             return;
//         }

//         Participant selectedTarget = hit.collider.GetComponentInParent<Participant>();

//         if (selectedTarget == null)
//         {
//             return;
//         }

//         TrySelectTarget(selectedTarget);
//     }

//     private void TrySelectTarget(Participant selectedTarget)
//     {
//         if (currentAttackData == null)
//         {
//             Debug.LogError("[CombatTargetSelectionManager] Current attack data is null.");
//             ClearSelection();
//             return;
//         }

//         bool isValidTarget = combatTargetProvider.IsValidManualTarget(
//             currentAttacker,
//             selectedTarget,
//             currentAttackData.TargetType);

//         if (!isValidTarget)
//         {
//             Debug.LogWarning(
//                 $"[CombatTargetSelectionManager] {selectedTarget.CombatantName} is not a valid target for {currentAttackData.TargetType}.");

//             return;
//         }

//         ClearSelection();

//         Debug.Log(
//             $"<color=#55FF88>[Target Selection]</color> Selected target: {selectedTarget.CombatantName}");

//         TargetSelected?.Invoke(selectedTarget);
//     }

//     private bool RequiresManualSelection(CombatActionTargetType targetType)
//     {
//         return targetType == CombatActionTargetType.SingleEnemy ||
//                targetType == CombatActionTargetType.SingleAlly;
//     }

//     private void ClearSelection()
//     {
//         currentAttacker = null;
//         currentAttackData = null;
//         IsSelectingTarget = false;
//     }
// }
