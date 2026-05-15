using UnityEngine;
using UnityEngine.UI; // Adicionado para controlar a barra de Stamina!

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        [Header("Sistema de Stamina")]
        public Slider barraDeStamina; // Arrastar o teu Slider da UI aqui
        public float staminaMaxima = 100f;
        public float consumoPorSegundo = 20f; // Quanta stamina gasta por segundo a correr
        public float recuperacaoPorSegundo = 15f; // Quão rápido recupera ao andar
        
        private float staminaAtual;
        private bool exausto = false; // Impede o jogador de correr até a barra encher
        private bool tentandoCorrer = false; // Guarda se estás a carregar na tecla Shift

        private void Start()
        {
            // Configura a barra de stamina no início
            staminaAtual = staminaMaxima;
            if (barraDeStamina != null)
            {
                barraDeStamina.maxValue = staminaMaxima;
                barraDeStamina.value = staminaAtual;
            }
        }

        private void Update()
        {
            // Se o jogo estiver pausado, não faz nada com a stamina
            if (MenuOpcoes.jogoPausado) return;

            // --- LÓGICA DE STAMINA ---
            // Se o jogador carrega no shift, ESTÁ a mexer-se, e NÃO está exausto
            if (tentandoCorrer && move != Vector2.zero && !exausto)
            {
                sprint = true; // Diz ao controlador do boneco para correr
                staminaAtual -= consumoPorSegundo * Time.deltaTime; // Gasta energia
                
                if (staminaAtual <= 0)
                {
                    staminaAtual = 0;
                    exausto = true; // Fica exausto!
                    sprint = false; // Força a parar de correr
                }
            }
            else
            {
                sprint = false; // Caminha normalmente
                
                // Recupera o fôlego
                if (staminaAtual < staminaMaxima)
                {
                    staminaAtual += recuperacaoPorSegundo * Time.deltaTime;
                    
                    if (staminaAtual >= staminaMaxima)
                    {
                        staminaAtual = staminaMaxima;
                        exausto = false; // Pode voltar a correr!
                    }
                }
            }

            // Atualiza a barra no ecrã
            if (barraDeStamina != null)
            {
                barraDeStamina.value = staminaAtual;
            }
        }

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            // Só deixa mexer a câmara se o jogo NÃO estiver pausado
            if(cursorInputForLook && !MenuOpcoes.jogoPausado)
            {
                LookInput(value.Get<Vector2>());
            }
            else
            {
                LookInput(Vector2.zero); // Força a câmara a ficar quieta
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }
#endif

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        } 

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            // Em vez de correr imediatamente, registamos apenas a INTENÇÃO de correr
            // O Update acima vai decidir se o boneco tem fôlego para isso ou não
            tentandoCorrer = newSprintState;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            // Se estiver em pausa, liberta e mostra o rato sempre!
            if (MenuOpcoes.jogoPausado)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !newState; // Garante que o rato fica invisível a jogar
            }
        }
    }
}