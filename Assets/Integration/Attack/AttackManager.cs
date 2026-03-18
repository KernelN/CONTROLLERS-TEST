using UnityEngine;

namespace Flame.Gameplay.Player
{
    [System.Serializable]
    public class AttackManager
    {
        static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
        [SerializeField] GameObject[] attackObjects;
        Animator animator;

        public void Set(Animator animator)
        {
            this.animator = animator;
        }
        public void OnAttackInput()
        {
            animator.SetBool(IsAttacking, true);
        }
        public void TurnOnAttack(int index)
        {
            bool isValidAttack = index > 0 && index < attackObjects.Length;
            if (!isValidAttack)
                animator.SetBool(IsAttacking, false);
            animator.applyRootMotion = isValidAttack;
            
            for (int i = 0; i < attackObjects.Length; i++)
                attackObjects[i].SetActive(i == index);
        }
    }
}