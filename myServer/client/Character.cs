using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using UnityEngine;

namespace client
{
    // Character _char = new Character(model,mat);
    // _char.change('hair','')
    public class Character
    {    
        public  Transform _model;    //character模型
        private Material _mat;       //Material
        private Gender _gender;
        //与列表对应的字符串
        Dictionary<string, List<GameObject>> PartMap = new Dictionary<string, List<GameObject>>();
        //存放有哪些part可以设置的DICT
        Dictionary<string, int> PartIndex = new Dictionary<string, int>();
        //保存当前的index
        Dictionary<string, int> CurrentPartIndex = new Dictionary<string, int>();
        
        // character object lists
        // male list 
        [HideInInspector]
        public CharacterObjectGroups male = new CharacterObjectGroups();
        // female list
        [HideInInspector]
        public CharacterObjectGroups female = new CharacterObjectGroups();
        // universal list
        [HideInInspector]
        public CharacterObjectListsAllGender allGender = new CharacterObjectListsAllGender();
        [HideInInspector]
        public List<GameObject> enabledObjects = new List<GameObject>();
        //构造函数 
        public Character(Transform model) {
            _model = model;
            // _mat = new Material();
            SetGender(Gender.Male);
        }
        //设置性别
        public void SetGender(Gender gender) {
            _gender = gender;
            // rebuild all lists
            BuildLists();
            // disable any enabled objects before clear
            if (enabledObjects.Count != 0)
            {
                foreach (GameObject g in enabledObjects)
                {
                    g.SetActive(false);
                }
            }
            // clear enabled objects list
            enabledObjects.Clear();
            CurrentPartIndex.Clear();
            if (gender == Gender.Male){
                // set default male character
                SetCharacter("Male_Head_All_Elements",0);
                SetCharacter("Male_01_Eyebrows",0);
                SetCharacter("Male_02_FacialHair",0);
                SetCharacter("Male_03_Torso",0);
                SetCharacter("Male_04_Arm_Upper_Right",0);
                SetCharacter("Male_05_Arm_Upper_Left",0);
                SetCharacter("Male_06_Arm_Lower_Right",0);
                SetCharacter("Male_07_Arm_Lower_Left",0);
                SetCharacter("Male_08_Hand_Right",0);
                SetCharacter("Male_09_Hand_Left",0);
                SetCharacter("Male_10_Hips",0);
                SetCharacter("Male_11_Leg_Right",0);
                SetCharacter("Male_12_Leg_Left",0);
            }
            else if (gender == Gender.Female){
                // set default female character
                SetCharacter("Male_Head_All_Elements",0);
                SetCharacter("Male_01_Eyebrows",0);
                // SetCharacter("Male_02_FacialHair",0);
                SetCharacter("Male_03_Torso",0);
                SetCharacter("Male_04_Arm_Upper_Right",0);
                SetCharacter("Male_05_Arm_Upper_Left",0);
                SetCharacter("Male_06_Arm_Lower_Right",0);
                SetCharacter("Male_07_Arm_Lower_Left",0);
                SetCharacter("Male_08_Hand_Right",0);
                SetCharacter("Male_09_Hand_Left",0);
                SetCharacter("Male_10_Hips",0);
                SetCharacter("Male_11_Leg_Right",0);
                SetCharacter("Male_12_Leg_Left",0);
            }
        }
        
        //设置外形
        public void SetCharacter(string part,int index) {
            CurrentPartIndex[part] = index;
            ActivateItem(PartMap[part],index);
        }
        //获得角色部件的数量index
        public int GetPartIndex(string part) {
            return PartIndex[part];
        }
        //获取角色有什么部件类型part
        public List<string> GetPartList() {
            return PartMap.Keys.ToList();
        }
        //获取角色性别
        public Gender GetGender() {
            return _gender;
        }
        //获取当前所选part index
        public Dictionary<string, int> GetCurrentPartIndex() {
            return CurrentPartIndex;
        }
        public int GetCurrentPartIndex(string part) {
            if (CurrentPartIndex.Keys.ToList().Contains(part)){
                return CurrentPartIndex[part];
            }
            return -1;
        }

        void ActivateItem(GameObject go)
        {
            // enable item
            go.SetActive(true);
            // add item to the enabled items list
            enabledObjects.Add(go);
        }

        void ActivateItem(List<GameObject> _list,int index) {
            foreach (var item in _list){
                item.SetActive(false);
            }
            _list[index].SetActive(true);
            enabledObjects.Add(_list[index]);
        }

        // called from the BuildLists method
        void BuildList(ref List<GameObject> targetList, string characterPart) {
            if (targetList == null) {
                targetList = new List<GameObject>();
            }
            Transform[] rootTransform = _model.GetComponentsInChildren<Transform>();

            // declare target root transform
            Transform targetRoot = null;

            // find character parts parent object in the scene
            foreach (Transform t in rootTransform)
            {
                if (t.gameObject.name == characterPart)
                {
                    targetRoot = t;
                    break;
                }
            }

            // clears targeted list of all objects
            targetList.Clear();

            // cycle through all child objects of the parent object
            for (int i = 0; i < targetRoot.childCount; i++)
            {
                // get child gameobject index i
                GameObject go = targetRoot.GetChild(i).gameObject;

                // disable child object
                go.SetActive(false);

                // add object to the targeted object list
                targetList.Add(go);
                // collect the material for the random character, only if null in the inspector;
                if (!_mat)
                {
                    if (go.GetComponent<SkinnedMeshRenderer>())
                        _mat = new Material(go.GetComponent<SkinnedMeshRenderer>().material);
                }
            }
            PartIndex[characterPart] = targetRoot.childCount;
            PartMap[characterPart] = targetList;
        }
        
        private void BuildLists()
        {
            //build out male lists
            BuildList(ref male.headAllElements, "Male_Head_All_Elements");
            BuildList(ref male.headNoElements, "Male_Head_No_Elements");
            BuildList(ref male.eyebrow, "Male_01_Eyebrows");
            BuildList(ref male.facialHair, "Male_02_FacialHair");
            BuildList(ref male.torso, "Male_03_Torso");
            BuildList(ref male.arm_Upper_Right, "Male_04_Arm_Upper_Right");
            BuildList(ref male.arm_Upper_Left, "Male_05_Arm_Upper_Left");
            BuildList(ref male.arm_Lower_Right, "Male_06_Arm_Lower_Right");
            BuildList(ref male.arm_Lower_Left, "Male_07_Arm_Lower_Left");
            BuildList(ref male.hand_Right, "Male_08_Hand_Right");
            BuildList(ref male.hand_Left, "Male_09_Hand_Left");
            BuildList(ref male.hips, "Male_10_Hips");
            BuildList(ref male.leg_Right, "Male_11_Leg_Right");
            BuildList(ref male.leg_Left, "Male_12_Leg_Left");

            //build out female lists
            BuildList(ref female.headAllElements, "Female_Head_All_Elements");
            BuildList(ref female.headNoElements, "Female_Head_No_Elements");
            BuildList(ref female.eyebrow, "Female_01_Eyebrows");
            BuildList(ref female.facialHair, "Female_02_FacialHair");
            BuildList(ref female.torso, "Female_03_Torso");
            BuildList(ref female.arm_Upper_Right, "Female_04_Arm_Upper_Right");
            BuildList(ref female.arm_Upper_Left, "Female_05_Arm_Upper_Left");
            BuildList(ref female.arm_Lower_Right, "Female_06_Arm_Lower_Right");
            BuildList(ref female.arm_Lower_Left, "Female_07_Arm_Lower_Left");
            BuildList(ref female.hand_Right, "Female_08_Hand_Right");
            BuildList(ref female.hand_Left, "Female_09_Hand_Left");
            BuildList(ref female.hips, "Female_10_Hips");
            BuildList(ref female.leg_Right, "Female_11_Leg_Right");
            BuildList(ref female.leg_Left, "Female_12_Leg_Left");

            // build out all gender lists
            BuildList(ref allGender.all_Hair, "All_01_Hair");
            BuildList(ref allGender.all_Head_Attachment, "All_02_Head_Attachment");
            BuildList(ref allGender.headCoverings_Base_Hair, "HeadCoverings_Base_Hair");
            BuildList(ref allGender.headCoverings_No_FacialHair, "HeadCoverings_No_FacialHair");
            BuildList(ref allGender.headCoverings_No_Hair, "HeadCoverings_No_Hair");
            BuildList(ref allGender.chest_Attachment, "All_03_Chest_Attachment");
            BuildList(ref allGender.back_Attachment, "All_04_Back_Attachment");
            BuildList(ref allGender.shoulder_Attachment_Right, "All_05_Shoulder_Attachment_Right");
            BuildList(ref allGender.shoulder_Attachment_Left, "All_06_Shoulder_Attachment_Left");
            BuildList(ref allGender.elbow_Attachment_Right, "All_07_Elbow_Attachment_Right");
            BuildList(ref allGender.elbow_Attachment_Left, "All_08_Elbow_Attachment_Left");
            BuildList(ref allGender.hips_Attachment, "All_09_Hips_Attachment");
            BuildList(ref allGender.knee_Attachement_Right, "All_10_Knee_Attachement_Right");
            BuildList(ref allGender.knee_Attachement_Left, "All_11_Knee_Attachement_Left");
            BuildList(ref allGender.elf_Ear, "Elf_Ear");
        }
    }
    
    
    // classe for keeping the lists organized, allows for simple switching from male/female objects
    [System.Serializable]
    public class CharacterObjectGroups
    {
        public List<GameObject> headAllElements;
        public List<GameObject> headNoElements;
        public List<GameObject> eyebrow;
        public List<GameObject> facialHair;
        public List<GameObject> torso;
        public List<GameObject> arm_Upper_Right;
        public List<GameObject> arm_Upper_Left;
        public List<GameObject> arm_Lower_Right;
        public List<GameObject> arm_Lower_Left;
        public List<GameObject> hand_Right;
        public List<GameObject> hand_Left;
        public List<GameObject> hips;
        public List<GameObject> leg_Right;
        public List<GameObject> leg_Left;
    }
    // classe for keeping the lists organized, allows for organization of the all gender items
    [System.Serializable]
    public class CharacterObjectListsAllGender
    {
        public List<GameObject> headCoverings_Base_Hair;
        public List<GameObject> headCoverings_No_FacialHair;
        public List<GameObject> headCoverings_No_Hair;
        public List<GameObject> all_Hair;
        public List<GameObject> all_Head_Attachment;
        public List<GameObject> chest_Attachment;
        public List<GameObject> back_Attachment;
        public List<GameObject> shoulder_Attachment_Right;
        public List<GameObject> shoulder_Attachment_Left;
        public List<GameObject> elbow_Attachment_Right;
        public List<GameObject> elbow_Attachment_Left;
        public List<GameObject> hips_Attachment;
        public List<GameObject> knee_Attachement_Right;
        public List<GameObject> knee_Attachement_Left;
        public List<GameObject> all_12_Extra;
        public List<GameObject> elf_Ear;
    }
    public enum Gender
    {
        Male,
        Female,
    }
    
}