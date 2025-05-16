using Assets.Scripts.TellStory.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCutLine : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Test("֪���İְ��������֪����ȥ�����" +
            "�������Ȱ���" +
            "֪��Ҫ�Ǵ��������������Ƥ�׵ġ�" +
            "�ְ�����ܵ���֪��������Ƥ����" +
            "û��ϵ��֪�����˵غ��š�" +
            "֪���Ͱְ�������������԰��" +
            "������ż��������������ǰ����" +
            "֪�����ڴ�����ż�ļ���ϣ����Ŷ�����һ����ǰ�ߡ�" +
            "���˺�һ�����������ż����Ȫ��ǰ��֪������������" +
            "֪�������ܿ��˿���˵���ף��ְ������أ�" +
            "̫���������ģ�֪�����ż���ûһ�������ͷ���ˡ�" +
            "֪�����ְֺ��������˹�����" +
            "֪��ǿ���ŵ�����һ���Ӿ�ӿ�˳�����" +
            "����ô�����Լ�������ܣ��ְ�����ൣ���㡣" +
            "���������ס֪����" +
            "��·��ʱ��һ��Ҫվ��ԭ�ز��������������Χ�Ĺ�����Ա��ȣ�" +
            "Ҫ˵��������ֺͰְ��������ϵ��ʽ��֪�����𣿰ְֽ�����ץס֪������˵��" +
            "�ص��Һ�֪��ϴ�˸���ˮ�裬����ϴ���ȫ��ʼ������" +
            "�ѵ�Ƥ���ַ��ˣ����Ų��Ǻ����أ��Ͻ�ȥ�Ҷ���ҽ���ɡ����迴��֪��˵��" +
            "���ˣ����ˣ�֪���Ͱְ�����һ����������ҽԺ��" +
            "����ҽ������ȫ��������֪��һ����������һ�߸�ҽ��˵��" +
            "����̫�ർ��Ƥ���������Ƥ��ȱ��Ӫ��ʱ����������ƣ�ͻ�ѹ����ʱ���������Ƥ�ס�" +
            "ҽ����ϸ�۲���֪����Ƥ����˵��");
    }

    void Test(string line)
    {
        AsrMain.usrDicPath = Application.persistentDataPath + "/jiebaConfig/usr_dic.txt";
        foreach (string r in AsrMain.resourceFiles)
        {
            AsrMain.requests[r] = new WWW(ResourceUtils.getReaderPath(r));
        }
        while (!AsrMain.allRequestsDone()) { }

        AsrMain.InitJieba();
        AsrMain asrMain = GetComponent<AsrMain>();
        string re = asrMain.cutLine(line);
        print(re);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
