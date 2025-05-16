 # coding=utf-8

import os
import json
import io
def transStr():
    # 获取目标文件夹的路径
    filedir = os.getcwd()
    # 获取文件夹中的文件名称列表
    filenames = os.listdir(filedir)
    # 遍历文件名
    for filename in filenames:
        filepath = filedir + '/' + filename
        print(filepath)
        # if (not filepath.endswith('json')):
        #     continue
        if (not filename=="Experiment_two_scenes.json"):
            continue

        after = []
        # 打开文件取出数据并修改，然后存入变量
        with open(filepath, 'r',encoding='utf-8') as f:
            data = json.load(f)
            print(type(data))
            list = data["keyFrames"]
            for zidian in list:
# pos scale
                if(checkmin(zidian['startpos'])and checkmin(zidian['endpos'])):
                    zidian['startpos'] = transList(zidian['startpos'], 10)
                    zidian['endpos'] = transList(zidian['endpos'], 10)


                if(zidian['name']=='RedHeadDuck' or
                 zidian['name']=='MallardDuck'):
                    for index in range(len(zidian['startscale'])):
                        zidian['startscale'][index]=2
                        zidian['endscale'][index]=2

                if (zidian['name'] == 'Apache'):
                    for index in range(len(zidian['startscale'])):
                        zidian['startscale'][index] = 5
                        zidian['endscale'][index] = 5


                    zidian['startrotation'][1] = -90
                    zidian['endrotation'][1] = -90

                if (zidian['name'] == 'Boeing'):
                    for index in range(len(zidian['startscale'])):
                        zidian['startscale'][index] = 5
                        zidian['endscale'][index] = 5

                if(zidian['name']=='Hunter'):
                    for index in range(len(zidian['startscale'])):
                        zidian['startscale'][index] = 1.5
                        zidian['endscale'][index] = 1.5
                    zidian['startrotation'][1] = 90
                    zidian['endrotation'][1] = 90

                if(zidian['name']=='TestClothes_Boy'):
                    for index in range(len(zidian['startscale'])):
                        zidian['startscale'][index] = 0.8
                        zidian['endscale'][index] = 0.8
                    zidian['startpos'][1]=0.3
                    zidian['endpos'][1]=0.3


                if (zidian['name'] == 'TestClothes_Girl'):
                    for index in range(len(zidian['startscale'])):
                        zidian['startscale'][index] = 0.9
                        zidian['endscale'][index] = 0.9

                if (zidian['name'] == 'RubberDuck'):
                    for index in range(len(zidian['startscale'])):
                        zidian['startscale'][index] = 1
                        zidian['endscale'][index] = 1


                if(zidian['name']=='Boy'):
                    zidian['name']='TestClothes_Boy'
                    zidian['goname']=zidian['goname'].replace('Boy','TestClothes_Boy')
                    if(zidian['content']=='Walk'):
                        zidian['content']='TestClothes_Boy_Walk'

                if (zidian['name'] == 'Girl'):
                    zidian['name'] = 'TestClothes_Girl'
                    zidian['goname'] = zidian['goname'].replace('Girl', 'TestClothes_Girl')
                    if (zidian['content'] == 'Walk'):
                        zidian['content'] = 'TestClothes_Girl_Walk'

                if(zidian['name']=='Hunter'):
                    if(zidian['content']=='Walk'):
                        zidian['content']='Hunter_walk'
                    if(zidian['content']=='Shoot'):
                        zidian['content']='Hunter_shoot'


                    # zidian['startrotation'][0] = 0
                    # zidian['endrotation'][0] = 0
                partNameList=['Hat','Jacket','Pants','Shoes']
                partPreList=[]
                if('clothes'in zidian.keys()):
                    clothes=zidian['clothes']
                    if(clothes!=''):
                        for part in partNameList:
                            partPreList.append(clothes[part])
                        zidian['partNameList'] = partNameList
                        zidian['partPreList'] = partPreList
                    else:
                        zidian['partNameList']=[]
                        zidian['partPreList']=[]
                    zidian.pop('clothes')






            after = data


        # 打开文件并覆盖写入修改后内容
        with open(filepath, 'w+',encoding='utf-8') as f:
            json.dump(after, f,ensure_ascii=False)

def transList(templist,factory=1):
    reList=[]
    for index in range(len(templist)):
        reList.append(float(templist[index])*factory)
    return reList

def checkmin(list):
    for index in range(len(list)):
        if float(list[index])>1 or float(list[index])<-1:
            return False
    return True



if __name__ == '__main__':
    transStr()
