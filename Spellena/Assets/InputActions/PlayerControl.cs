//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.5.1
//     from Assets/InputActions/PlayerControl.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerControl: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControl()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControl"",
    ""maps"": [
        {
            ""name"": ""PlayerActions"",
            ""id"": ""664f7515-ec3a-4097-ae3f-4151f9fd6a95"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""014e639d-438e-4502-aaf9-878ae67f0483"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""1ccbb9cf-1dd8-4a03-9cf2-639da2bd1827"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MouseButton"",
                    ""type"": ""Value"",
                    ""id"": ""c688387f-6e32-457f-b9d8-80f705424874"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MouseButton2"",
                    ""type"": ""Value"",
                    ""id"": ""d461b346-707c-4314-bafe-14410b1a730d"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Run"",
                    ""type"": ""Value"",
                    ""id"": ""54b3753b-b01d-4f90-bbb5-32b7c43bbcd2"",
                    ""expectedControlType"": ""Key"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Sit"",
                    ""type"": ""Value"",
                    ""id"": ""fac7d23a-a706-4d21-89a4-f1497b9704f5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Interaction"",
                    ""type"": ""Button"",
                    ""id"": ""873478a1-3497-4f82-bd4b-2c6392f85f81"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ButtonCancel"",
                    ""type"": ""Button"",
                    ""id"": ""3374d651-0ef2-408c-9e35-633cf05bf18e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Skill1"",
                    ""type"": ""Button"",
                    ""id"": ""9e48be17-8905-45ba-81b9-fdcfe24d91ec"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Skill2"",
                    ""type"": ""Button"",
                    ""id"": ""b0877e92-11da-4ef6-ab74-0cdd223915f0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Skill3"",
                    ""type"": ""Button"",
                    ""id"": ""467c893b-c415-468a-be3f-10ed8f2b1d45"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Skill4"",
                    ""type"": ""Button"",
                    ""id"": ""6fd97b47-a85c-43fa-bc29-5a939870a496"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Fly"",
                    ""type"": ""Value"",
                    ""id"": ""749cf095-5f8b-4187-84e8-fe44d0a84e3d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""466d6075-1743-42c7-bbe8-b687f1d66cea"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""Skill1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bac000d8-8a3f-4ad1-9c84-9bfbd4f89301"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""Skill2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""fecc2e40-6685-476c-8853-4ed615168e8c"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""Skill3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bcd01adf-f4e7-4732-94be-ca4dbb6f711d"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""Skill4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""54943ecb-8cf2-4c9c-aa1a-fa42c597cca7"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""48823ca0-edd6-4371-9e0e-c595ae04dc1a"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""Sit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b3bcd021-4e67-433b-9fb6-83046959a049"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""Interaction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cf50e4ce-67b0-4ac3-aee5-7bcdbebe8d8c"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""1ffa3ecb-6de2-4e8d-8e4b-63874798d58e"",
                    ""path"": ""2DVector(mode=2)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""f09c6994-243f-48b9-8817-117a7f3efa2a"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""eebbe454-8759-4844-831e-f2bc56e1ac55"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""fe347d4b-9141-438e-abac-c1972ff9dc92"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""96552ac3-791b-435e-afe3-fbed2cc00c05"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""4794f8e8-bb03-416e-bb27-c5fe29e31dd3"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""MouseButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2f73b243-9d85-4f22-901f-ea0bc3622069"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""ButtonCancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cd90ea70-8c5e-4378-be66-559487dc12d4"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""MouseButton2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""aaab07d6-b65b-46de-bef8-2070533076fd"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""PC"",
                    ""action"": ""Fly"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""PC"",
            ""bindingGroup"": ""PC"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // PlayerActions
        m_PlayerActions = asset.FindActionMap("PlayerActions", throwIfNotFound: true);
        m_PlayerActions_Move = m_PlayerActions.FindAction("Move", throwIfNotFound: true);
        m_PlayerActions_Jump = m_PlayerActions.FindAction("Jump", throwIfNotFound: true);
        m_PlayerActions_MouseButton = m_PlayerActions.FindAction("MouseButton", throwIfNotFound: true);
        m_PlayerActions_MouseButton2 = m_PlayerActions.FindAction("MouseButton2", throwIfNotFound: true);
        m_PlayerActions_Run = m_PlayerActions.FindAction("Run", throwIfNotFound: true);
        m_PlayerActions_Sit = m_PlayerActions.FindAction("Sit", throwIfNotFound: true);
        m_PlayerActions_Interaction = m_PlayerActions.FindAction("Interaction", throwIfNotFound: true);
        m_PlayerActions_ButtonCancel = m_PlayerActions.FindAction("ButtonCancel", throwIfNotFound: true);
        m_PlayerActions_Skill1 = m_PlayerActions.FindAction("Skill1", throwIfNotFound: true);
        m_PlayerActions_Skill2 = m_PlayerActions.FindAction("Skill2", throwIfNotFound: true);
        m_PlayerActions_Skill3 = m_PlayerActions.FindAction("Skill3", throwIfNotFound: true);
        m_PlayerActions_Skill4 = m_PlayerActions.FindAction("Skill4", throwIfNotFound: true);
        m_PlayerActions_Fly = m_PlayerActions.FindAction("Fly", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // PlayerActions
    private readonly InputActionMap m_PlayerActions;
    private List<IPlayerActionsActions> m_PlayerActionsActionsCallbackInterfaces = new List<IPlayerActionsActions>();
    private readonly InputAction m_PlayerActions_Move;
    private readonly InputAction m_PlayerActions_Jump;
    private readonly InputAction m_PlayerActions_MouseButton;
    private readonly InputAction m_PlayerActions_MouseButton2;
    private readonly InputAction m_PlayerActions_Run;
    private readonly InputAction m_PlayerActions_Sit;
    private readonly InputAction m_PlayerActions_Interaction;
    private readonly InputAction m_PlayerActions_ButtonCancel;
    private readonly InputAction m_PlayerActions_Skill1;
    private readonly InputAction m_PlayerActions_Skill2;
    private readonly InputAction m_PlayerActions_Skill3;
    private readonly InputAction m_PlayerActions_Skill4;
    private readonly InputAction m_PlayerActions_Fly;
    public struct PlayerActionsActions
    {
        private @PlayerControl m_Wrapper;
        public PlayerActionsActions(@PlayerControl wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_PlayerActions_Move;
        public InputAction @Jump => m_Wrapper.m_PlayerActions_Jump;
        public InputAction @MouseButton => m_Wrapper.m_PlayerActions_MouseButton;
        public InputAction @MouseButton2 => m_Wrapper.m_PlayerActions_MouseButton2;
        public InputAction @Run => m_Wrapper.m_PlayerActions_Run;
        public InputAction @Sit => m_Wrapper.m_PlayerActions_Sit;
        public InputAction @Interaction => m_Wrapper.m_PlayerActions_Interaction;
        public InputAction @ButtonCancel => m_Wrapper.m_PlayerActions_ButtonCancel;
        public InputAction @Skill1 => m_Wrapper.m_PlayerActions_Skill1;
        public InputAction @Skill2 => m_Wrapper.m_PlayerActions_Skill2;
        public InputAction @Skill3 => m_Wrapper.m_PlayerActions_Skill3;
        public InputAction @Skill4 => m_Wrapper.m_PlayerActions_Skill4;
        public InputAction @Fly => m_Wrapper.m_PlayerActions_Fly;
        public InputActionMap Get() { return m_Wrapper.m_PlayerActions; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActionsActions set) { return set.Get(); }
        public void AddCallbacks(IPlayerActionsActions instance)
        {
            if (instance == null || m_Wrapper.m_PlayerActionsActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_PlayerActionsActionsCallbackInterfaces.Add(instance);
            @Move.started += instance.OnMove;
            @Move.performed += instance.OnMove;
            @Move.canceled += instance.OnMove;
            @Jump.started += instance.OnJump;
            @Jump.performed += instance.OnJump;
            @Jump.canceled += instance.OnJump;
            @MouseButton.started += instance.OnMouseButton;
            @MouseButton.performed += instance.OnMouseButton;
            @MouseButton.canceled += instance.OnMouseButton;
            @MouseButton2.started += instance.OnMouseButton2;
            @MouseButton2.performed += instance.OnMouseButton2;
            @MouseButton2.canceled += instance.OnMouseButton2;
            @Run.started += instance.OnRun;
            @Run.performed += instance.OnRun;
            @Run.canceled += instance.OnRun;
            @Sit.started += instance.OnSit;
            @Sit.performed += instance.OnSit;
            @Sit.canceled += instance.OnSit;
            @Interaction.started += instance.OnInteraction;
            @Interaction.performed += instance.OnInteraction;
            @Interaction.canceled += instance.OnInteraction;
            @ButtonCancel.started += instance.OnButtonCancel;
            @ButtonCancel.performed += instance.OnButtonCancel;
            @ButtonCancel.canceled += instance.OnButtonCancel;
            @Skill1.started += instance.OnSkill1;
            @Skill1.performed += instance.OnSkill1;
            @Skill1.canceled += instance.OnSkill1;
            @Skill2.started += instance.OnSkill2;
            @Skill2.performed += instance.OnSkill2;
            @Skill2.canceled += instance.OnSkill2;
            @Skill3.started += instance.OnSkill3;
            @Skill3.performed += instance.OnSkill3;
            @Skill3.canceled += instance.OnSkill3;
            @Skill4.started += instance.OnSkill4;
            @Skill4.performed += instance.OnSkill4;
            @Skill4.canceled += instance.OnSkill4;
            @Fly.started += instance.OnFly;
            @Fly.performed += instance.OnFly;
            @Fly.canceled += instance.OnFly;
        }

        private void UnregisterCallbacks(IPlayerActionsActions instance)
        {
            @Move.started -= instance.OnMove;
            @Move.performed -= instance.OnMove;
            @Move.canceled -= instance.OnMove;
            @Jump.started -= instance.OnJump;
            @Jump.performed -= instance.OnJump;
            @Jump.canceled -= instance.OnJump;
            @MouseButton.started -= instance.OnMouseButton;
            @MouseButton.performed -= instance.OnMouseButton;
            @MouseButton.canceled -= instance.OnMouseButton;
            @MouseButton2.started -= instance.OnMouseButton2;
            @MouseButton2.performed -= instance.OnMouseButton2;
            @MouseButton2.canceled -= instance.OnMouseButton2;
            @Run.started -= instance.OnRun;
            @Run.performed -= instance.OnRun;
            @Run.canceled -= instance.OnRun;
            @Sit.started -= instance.OnSit;
            @Sit.performed -= instance.OnSit;
            @Sit.canceled -= instance.OnSit;
            @Interaction.started -= instance.OnInteraction;
            @Interaction.performed -= instance.OnInteraction;
            @Interaction.canceled -= instance.OnInteraction;
            @ButtonCancel.started -= instance.OnButtonCancel;
            @ButtonCancel.performed -= instance.OnButtonCancel;
            @ButtonCancel.canceled -= instance.OnButtonCancel;
            @Skill1.started -= instance.OnSkill1;
            @Skill1.performed -= instance.OnSkill1;
            @Skill1.canceled -= instance.OnSkill1;
            @Skill2.started -= instance.OnSkill2;
            @Skill2.performed -= instance.OnSkill2;
            @Skill2.canceled -= instance.OnSkill2;
            @Skill3.started -= instance.OnSkill3;
            @Skill3.performed -= instance.OnSkill3;
            @Skill3.canceled -= instance.OnSkill3;
            @Skill4.started -= instance.OnSkill4;
            @Skill4.performed -= instance.OnSkill4;
            @Skill4.canceled -= instance.OnSkill4;
            @Fly.started -= instance.OnFly;
            @Fly.performed -= instance.OnFly;
            @Fly.canceled -= instance.OnFly;
        }

        public void RemoveCallbacks(IPlayerActionsActions instance)
        {
            if (m_Wrapper.m_PlayerActionsActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IPlayerActionsActions instance)
        {
            foreach (var item in m_Wrapper.m_PlayerActionsActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_PlayerActionsActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public PlayerActionsActions @PlayerActions => new PlayerActionsActions(this);
    private int m_PCSchemeIndex = -1;
    public InputControlScheme PCScheme
    {
        get
        {
            if (m_PCSchemeIndex == -1) m_PCSchemeIndex = asset.FindControlSchemeIndex("PC");
            return asset.controlSchemes[m_PCSchemeIndex];
        }
    }
    public interface IPlayerActionsActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnMouseButton(InputAction.CallbackContext context);
        void OnMouseButton2(InputAction.CallbackContext context);
        void OnRun(InputAction.CallbackContext context);
        void OnSit(InputAction.CallbackContext context);
        void OnInteraction(InputAction.CallbackContext context);
        void OnButtonCancel(InputAction.CallbackContext context);
        void OnSkill1(InputAction.CallbackContext context);
        void OnSkill2(InputAction.CallbackContext context);
        void OnSkill3(InputAction.CallbackContext context);
        void OnSkill4(InputAction.CallbackContext context);
        void OnFly(InputAction.CallbackContext context);
    }
}
