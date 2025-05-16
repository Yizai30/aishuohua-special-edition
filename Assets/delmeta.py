import os

allFiles=[]
rootdir = 'D:\Resources'

def getAllFile(path):
       allfileList=os.listdir(path)
       for file in allfileList:
              filePath=os.path.join(path,file)
              if os.path.isdir(filePath):
                     getAllFile(filePath)
              allFiles.append(filePath)
       return



if __name__=='__main__':
       path='D:\Resources'
       allfiles=getAllFile(path)
       for file in allFiles:
              if(file[-4:]=='meta'):
                     print(file)
                     os.remove(file)
              #print(file)
