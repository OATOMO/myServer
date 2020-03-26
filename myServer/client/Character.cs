using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace client
{
    // Character _char = new Character(model,mat);
    // _char.change('hair','')
    public class Character
    {
        private GameObject _model;    //character模型
        private Material _mat;       //Material
        private Gender _gender;
        //与列表对应的字符串
        Dictionary<string, List<GameObject>> PartMap;
        //存放有哪些part可以设置的DICT
        Dictionary<string, int> PartIndex;
        //保存当前的index
        Dictionary<string, int> CurrentPartIndex;
        
        // character object lists
        // male list 
        [HideInInspector]
        public CharacterObjectGroups male;
        // female list
        [HideInInspector]
        public CharacterObjectGroups female;
        // universal list
        [HideInInspector]
        public CharacterObjectListsAllGender allGender;
        [HideInInspector]
        public List<GameObject> enabledObjects = new List<GameObject>();
        //构造函数 
        public Character(GameObject model) {
            _model = model;
            _mat = new Material();
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
            if (gender == Gender.Male){
                // set default male character
                ActivateItem(male.headAllElements[0]);
                ActivateItem(male.eyebrow[0]);
                ActivateItem(male.facialHair[0]);
                ActivateItem(male.torso[0]);
                ActivateItem(male.arm_Upper_Right[0]);
                ActivateItem(male.arm_Upper_Left[0]);
                ActivateItem(male.arm_Lower_Right[0]);
                ActivateItem(male.arm_Lower_Left[0]);
                ActivateItem(male.hand_Right[0]);
                ActivateItem(male.hand_Left[0]);
                ActivateItem(male.hips[0]);
                ActivateItem(male.leg_Right[0]);
                ActivateItem(male.leg_Left[0]);
            }
            else if (gender == Gender.Female){
                // set default female character
                ActivateItem(female.headAllElements[0]);
                ActivateItem(female.eyebrow[0]);
                // ActivateItem(female.facialHair[0]);
                ActivateItem(female.torso[0]);
                ActivateItem(female.arm_Upper_Right[0]);
                ActivateItem(female.arm_Upper_Left[0]);
                ActivateItem(female.arm_Lower_Right[0]);
                ActivateItem(female.arm_Lower_Left[0]);
                ActivateItem(female.hand_Right[0]);
                ActivateItem(female.hand_Left[0]);
                ActivateItem(female.hips[0]);
                ActivateItem(female.leg_Right[0]);
                ActivateItem(female.leg_Left[0]);
            }
        }
        
        //设置外形
        public void SetCharacter(string part,int index) {
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
        void BuildList(List<GameObject> targetList, string characterPart) {
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
                        go.GetComponent<SkinnedMeshRenderer>().material.CopyPropertiesFromMaterial(_mat);
                }
            }
            PartIndex[characterPart] = targetRoot.childCount;
            PartMap[characterPart] = targetList;
        }
        
        private void BuildLists()
        {
            //build out male lists
            BuildList(male.headAllElements, "Male_Head_All_Elements");
            BuildList(male.headNoElements, "Male_Head_No_Elements");
            BuildList(male.eyebrow, "Male_01_Eyebrows");
            BuildList(male.facialHair, "Male_02_FacialHair");
            BuildList(male.torso, "Male_03_Torso");
            BuildList(male.arm_Upper_Right, "Male_04_Arm_Upper_Right");
            BuildList(male.arm_Upper_Left, "Male_05_Arm_Upper_Left");
            BuildList(male.arm_Lower_Right, "Male_06_Arm_Lower_Right");
            BuildList(male.arm_Lower_Left, "Male_07_Arm_Lower_Left");
            BuildList(male.hand_Right, "Male_08_Hand_Right");
            BuildList(male.hand_Left, "Male_09_Hand_Left");
            BuildList(male.hips, "Male_10_Hips");
            BuildList(male.leg_Right, "Male_11_Leg_Right");
            BuildList(male.leg_Left, "Male_12_Leg_Left");

            //build out female lists
            BuildList(female.headAllElements, "Female_Head_All_Elements");
            BuildList(female.headNoElements, "Female_Head_No_Elements");
            BuildList(female.eyebrow, "Female_01_Eyebrows");
            BuildList(female.facialHair, "Female_02_FacialHair");
            BuildList(female.torso, "Female_03_Torso");
            BuildList(female.arm_Upper_Right, "Female_04_Arm_Upper_Right");
            BuildList(female.arm_Upper_Left, "Female_05_Arm_Upper_Left");
            BuildList(female.arm_Lower_Right, "Female_06_Arm_Lower_Right");
            BuildList(female.arm_Lower_Left, "Female_07_Arm_Lower_Left");
            BuildList(female.hand_Right, "Female_08_Hand_Right");
            BuildList(female.hand_Left, "Female_09_Hand_Left");
            BuildList(female.hips, "Female_10_Hips");
            BuildList(female.leg_Right, "Female_11_Leg_Right");
            BuildList(female.leg_Left, "Female_12_Leg_Left");

            // build out all gender lists
            BuildList(allGender.all_Hair, "All_01_Hair");
            BuildList(allGender.all_Head_Attachment, "All_02_Head_Attachment");
            BuildList(allGender.headCoverings_Base_Hair, "HeadCoverings_Base_Hair");
            BuildList(allGender.headCoverings_No_FacialHair, "HeadCoverings_No_FacialHair");
            BuildList(allGender.headCoverings_No_Hair, "HeadCoverings_No_Hair");
            BuildList(allGender.chest_Attachment, "All_03_Chest_Attachment");
            BuildList(allGender.back_Attachment, "All_04_Back_Attachment");
            BuildList(allGender.shoulder_Attachment_Right, "All_05_Shoulder_Attachment_Right");
            BuildList(allGender.shoulder_Attachment_Left, "All_06_Shoulder_Attachment_Left");
            BuildList(allGender.elbow_Attachment_Right, "All_07_Elbow_Attachment_Right");
            BuildList(allGender.elbow_Attachment_Left, "All_08_Elbow_Attachment_Left");
            BuildList(allGender.hips_Attachment, "All_09_Hips_Attachment");
            BuildList(allGender.knee_Attachement_Right, "All_10_Knee_Attachement_Right");
            BuildList(allGender.knee_Attachement_Left, "All_11_Knee_Attachement_Left");
            BuildList(allGender.elf_Ear, "Elf_Ear");
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