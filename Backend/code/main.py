from typing import List, Union, Optional
from fastapi import FastAPI, File, UploadFile, Form, BackgroundTasks
from starlette.responses import FileResponse
import os
import io
import json
from fastapi import FastAPI, Request, Response, status
from fastapi.exceptions import RequestValidationError
from fastapi.responses import JSONResponse, StreamingResponse, PlainTextResponse
import logging
import pymongo
import uuid
import uvicorn
import random
import string
from bson.objectid import ObjectId


from datetime import datetime, timedelta

from config import MONGODB_ADDRESS, SITEHOST

client = pymongo.MongoClient(MONGODB_ADDRESS, connect=False)
db = client['AnimationSource']

from fastapi_redis_cache import FastApiRedisCache, cache
from sqlalchemy.orm import Session

LOCAL_REDIS_URL = "redis://:passwd@127.0.0.1:6380"

app = FastAPI()


import hanlp

@app.on_event("startup")
def startup():
    hanlp.pretrained.srl.ALL
    global srl
    srl = hanlp.load('CPB3_SRL_ELECTRA_SMALL')
    pass
    #redis_cache = FastApiRedisCache()
    '''redis_cache.init(
        host_url=os.environ.get("REDIS_URL", LOCAL_REDIS_URL),
        prefix="myapi-cache",
        response_header="X-MyAPI-Cache",
        ignore_arg_types=[Request, Session]
    )'''

@app.get("/api/")
async def root():
    return {"message": "Hello World"}

def generate_preview(input_path, output_path):
    raw_cmd = """
width=384
height=216
frame_count=$(ffprobe -v error -show_entries format=duration {inputpath} -of default=noprint_wrappers=1:nokey=1)
frame_target=$( expr ${frame_count%.*} / 10 + 1)
ffmpeg -i {inputpath} -max_muxing_queue_size 999999 -an -qscale:v 1 -vframes 10 -f image2pipe -vcodec ppm \
    -vf "fps=1/$frame_target, scale=iw*min($width/iw\,$height/ih):ih*min($width/iw\,$height/ih):flags=lanczos, pad=$width:$height:($width-iw*min($width/iw\,$height/ih))/2:($height-ih*min($width/iw\,$height/ih))/2, unsharp=5:5:0.5:5:5:0.5" -\
| ffmpeg -y -framerate 1 -i pipe:0 -c:v libx264 -profile:v baseline -level 3.0 -tune stillimage -r 30 -pix_fmt yuv420p -max_muxing_queue_size 999999 {outputpath}
"""
    cmd = raw_cmd.replace("{inputpath}", input_path).replace("{outputpath}", output_path)
    os.system(cmd)

def writeUploadFileToDisk(uploadfile, name):
    fn = os.path.join("uploadedFiles", os.path.basename(name))
    open(fn, "wb").write(uploadfile.file.read())

@app.exception_handler(RequestValidationError)
async def validation_exception_handler(request: Request, exc: RequestValidationError):
	exc_str = f'{exc}'.replace('\n', ' ').replace('   ', ' ')
	logging.error(f"{request}: {exc_str}")
	content = {'status_code': 10422, 'message': exc_str, 'data': None}
	return JSONResponse(content=content, status_code=status.HTTP_422_UNPROCESSABLE_ENTITY)

@app.post("/api/report/")
async def report(reportText: Optional[str] = Form(None), pic1: UploadFile = File(default=None), pic2: UploadFile = File(default=None), pic3: UploadFile = File(default=None) ):
    pics = []
    if pic1 != None:
        pic1name = str(uuid.uuid4())
        writeUploadFileToDisk(pic1, f"{pic1name}_pic")
        pics.append(pic1name)
    if pic2 != None:
        pic2name = str(uuid.uuid4())
        writeUploadFileToDisk(pic2, f"{pic2name}_pic")
        pics.append(pic2name)
    if pic3 != None:
        pic3name = str(uuid.uuid4())
        writeUploadFileToDisk(pic3, f"{pic3name}_pic")
        pics.append(pic3name)
    with open(f"uploadedFiles/{str(uuid.uuid4())}_report.txt", "w") as report:
        report.write(",".join(pics) + f"\nreportText: {reportText}\n")
    return

@app.get("/api/data/preview/{data_type}", responses={204: {"model": None}})
#@cache(expire=30)
async def get_data_preview(data_type: str):
    return {"Results": list(db["Preview"].find({"Type": data_type}, {"_id": False}))}

@app.get("/api/data/actor/{name}")
#@cache(expire=30)
async def get_actors(name: str):
    actor_list = list(db["Actor"].find({"Name": name}, {"_id": False}))
    for actor in actor_list:
        actor['Animation'] = [str(a) for a in actor['Animation']] if actor['Animation'] is not None else []
    return {"Results": actor_list}

@app.get("/api/data/animation/{objId}")
#@cache(expire=30)
async def get_animations(objId: str):
    anim_list = list(db["Animation"].find({"_id": ObjectId(objId)}, {"_id": False}))
    for anim in anim_list:
        anim['ActorList'] = [str(a) for a in anim['ActorList']] if anim['ActorList'] is not None else []
    return {"Results": anim_list}

@app.get("/api/uploadedFiles/{filename}")
async def get_uploaded_files(filename):
    if os.path.exists(os.path.join("uploadedFiles",filename)):
        return FileResponse(os.path.join("uploadedFiles",filename), media_type='video/mp4',filename=filename)
    else:
        # print("Warning: file", filename,"not found")
        return ""

@app.get("/api/uploadedFileProcessed/{filename}")
async def get_uploaded_file_processed(filename):
    if not os.path.exists(os.path.join("uploadedFiles", filename + ".lock")):
        return {"videoUrl": f"{SITEHOST}api/uploadedFiles/{filename}"}
    else:
        return {"videoUrl": ""}

@app.get("/api/myStories/{username}")
#@cache(expire=5)
async def get_my_stories(username):
    storiesList = list(db["UserData"].find({"userName": username}, {"_id": False, "userName": False}))
    storiesList.reverse()
    return {"stories": storiesList}

def get_video_duration(vfile):
    try:
        vinfo = vfile + ".info"
        os.system(f"ffprobe -v error -select_streams v:0 -show_entries stream=duration -of default=noprint_wrappers=1:nokey=1 {vfile} > {vinfo}")
        duration = open(vinfo, "r").read()
    except:
        duration = "未知"
    if "." in duration:
        return duration[:duration.find(".")]
    else:
        return duration

@app.get("/api/updateMyStories/{username}")
async def update_my_stories(username):
    storiesList = list(db["UserData"].find({"userName": username}))
    for story in storiesList:
        relativeVfile = story["videoUrl"].replace("https://test1.deepsoft-tech.com:8443/api/", "").replace("https://kisstherain.top/api/", "")
        if not "时长" in story["createTime"]:
            duration = get_video_duration(relativeVfile)
            db["UserData"].update_one({'_id': story["_id"]}, {"$set": {"createTime": story["createTime"] + "\n时长：" + duration + " 秒"}}, upsert=False)
    storiesList = list(db["UserData"].find({"userName": username}, {"_id": False, "userName": False}))
    return {"stories": storiesList}

@app.post("/api/fix/")
async def fix_and_save(username: str = Form(), video: UploadFile = File(default=None)):
    filename = str(uuid.uuid4())
    writeUploadFileToDisk(video, filename + "_raw.mp4")
    if os.path.getsize(f"uploadedFiles/{filename}_raw.mp4") < 200 * 1024:
        os.system(f"ffmpeg -i uploadedFiles/{filename}_raw.mp4 -c:v libx264 -crf 23 -profile:v baseline -level 3.0 -pix_fmt yuv420p -c:a aac -ac 2 -b:a 128k -movflags faststart -vf scale=\"1280:720\" -max_muxing_queue_size 999999 uploadedFiles/{filename}.mp4 -y -vsync 2")
    else:
        os.system(f"ffmpeg -i uploadedFiles/{filename}_raw.mp4 -c:v libx264 -crf 23 -profile:v baseline -level 3.0 -pix_fmt yuv420p -c:a aac -ac 2 -b:a 128k -movflags faststart -vf scale=\"1280:720\" -max_muxing_queue_size 999999 uploadedFiles/{filename}.mp4 -y")
    generate_preview(f"uploadedFiles/{filename}.mp4", f"uploadedFiles/{filename}_preview.mp4")
    duration = get_video_duration(f"uploadedFiles/{filename}.mp4")
    db["UserData"].insert_one({
        "userName": username,
        "createTime": (datetime.utcnow() + timedelta(hours=8)).strftime('创建于%Y年%m月%d日\n%H时%M分%S秒') + "\n时长：" + duration + " 秒",
        "previewUrl": f"{SITEHOST}api/uploadedFiles/{filename}_preview.mp4",
        "videoUrl": f"{SITEHOST}api/uploadedFiles/{filename}.mp4",
    })
    #print("return", f"{SITEHOST}api/uploadedFiles/{filename}.mp4")
    return {"videoUrl": f"{SITEHOST}api/uploadedFiles/{filename}.mp4"}

@app.post("/api/fix2/")
async def fix_and_save2(username: str = Form(), video: UploadFile = File(default=None), audioTimestamps: str = Form(), audioFiles: List[UploadFile] = File(default=None)):
    
    l = len(audioFiles)
    fns = []
    timestamps = audioTimestamps.split(";")[:l]
    for af in audioFiles:
        fn = str(uuid.uuid4())
        writeUploadFileToDisk(af, fn + ".wav")
        fns.append(fn)
    for i in range(l):
        print(timestamps[i], " -> ", fns[i])
    mergedFn = str(uuid.uuid4())
    args = []
    for fn in fns:
        args.append(f"-i uploadedFiles/{fn}.wav")
    args.append("-filter_complex")
    fcargs = []
    stnames = []
    for i in range(l):
        stname = ''.join(random.choice(string.ascii_uppercase) for _ in range(5))
        fcargs.append(f"[{i}]adelay=delays={timestamps[i]}[{stname}];");
        stnames.append(stname)
    fcargs.append("".join(f"[{n}]" for n in stnames) + f"amix=inputs={l}")
    args.append("\"" + "".join(fcargs) + "\"")
    args.append(f"-vsync 2 -y uploadedFiles/{mergedFn}.wav")
    ffmpegcmd = "ffmpeg " + " ".join(args)
    print("ffmpegcmd1:", ffmpegcmd)
    os.system(ffmpegcmd)
    
    filename = str(uuid.uuid4())
    writeUploadFileToDisk(video, filename + "_raw.mp4")
    if os.path.getsize(f"uploadedFiles/{filename}_raw.mp4") < 200 * 1024:
        os.system(f"ffmpeg -i uploadedFiles/{filename}_raw.mp4 -c:v libx264 -crf 23 -profile:v baseline -level 3.0 -pix_fmt yuv420p -c:a aac -ac 2 -b:a 128k -movflags faststart -vf scale=\"1280:720\" uploadedFiles/{filename}_.mp4 -y -vsync 2")
    else:
        os.system(f"ffmpeg -i uploadedFiles/{filename}_raw.mp4 -c:v libx264 -crf 23 -profile:v baseline -level 3.0 -pix_fmt yuv420p -c:a aac -ac 2 -b:a 128k -movflags faststart -vf scale=\"1280:720\" uploadedFiles/{filename}_.mp4 -y")
    generate_preview(f"uploadedFiles/{filename}_.mp4", f"uploadedFiles/{filename}_preview.mp4")
    ffmpegcmd = f"ffmpeg -i uploadedFiles/{filename}_.mp4 -i uploadedFiles/{mergedFn}.wav -map 0:v:0 -map 1:a:0 -c:v libx264 -crf 23 -profile:v baseline -level 3.0 -pix_fmt yuv420p -c:a aac -ac 2 -b:a 128k -movflags faststart -y -shortest uploadedFiles/{filename}.mp4 -y"
    print("ffmpegcmd2:", ffmpegcmd)
    os.system(ffmpegcmd)
    db["UserData"].insert_one({
        "userName": username,
        "createTime": (datetime.utcnow() + timedelta(hours=8)).strftime('创建于%Y年%m月%d日\n%H时%M分%S秒'),
        "previewUrl": f"{SITEHOST}api/uploadedFiles/{filename}_preview.mp4",
        "videoUrl": f"{SITEHOST}api/uploadedFiles/{filename}.mp4",
    })
    return {"videoUrl": f"{SITEHOST}api/uploadedFiles/{filename}.mp4"}


@app.post("/api/upload")
async def upload(username: str = Form(), video: UploadFile = File(default=None)):
    filename = str(uuid.uuid4())
    writeUploadFileToDisk(video, filename + ".mp4")
    generate_preview(f"uploadedFiles/{filename}.mp4", f"uploadedFiles/{filename}_preview.mp4")
    db["UserData"].insert_one({
        "userName": username,
        "createTime": (datetime.utcnow() + timedelta(hours=8)).strftime('创建于%Y年%m月%d日\n%H时%M分%S秒'),
        "previewUrl": f"{SITEHOST}api/uploadedFiles/{filename}_preview.mp4",
        "videoUrl": f"{SITEHOST}api/uploadedFiles/{filename}.mp4",
    })
    print("return", f"{SITEHOST}api/uploadedFiles/{filename}.mp4")
    return {"videoUrl": f"{SITEHOST}api/uploadedFiles/{filename}.mp4"}


@app.post("/api/deleteStory/{username}")
async def post_delete_story(username, videoUrl: str = Form()):
    db["UserData"].delete_one({"userName": username, "videoUrl": videoUrl})
    return

async def combine(vfname: str, afname:str, nfname:str):
    open(f"uploadedFiles/{nfname}_{vfname}.mp4.lock", "w").write("Combine")
    if not os.path.exists(f"uploadedFiles/{vfname}_crop.mp4"):
        crop_param = get_crop_param(f"ffmpeg -i uploadedFiles/{vfname}.mp4 -vframes 10 -vf cropdetect -f null -")
        os.system(f"ffmpeg -i uploadedFiles/{vfname}.mp4 -vf {crop_param} -y uploadedFiles/{vfname}_crop.mp4")
    os.system(f"ffmpeg -i uploadedFiles/{vfname}_crop.mp4 -vf \"scale=1280:720:force_original_aspect_ratio=decrease,pad=1280:720:(ow-iw)/2:(oh-ih)/2,setsar=1\" -y uploadedFiles/{nfname}_pad.mp4")
    os.system(f"ffmpeg -i uploadedFiles/{nfname}_pad.mp4 -vf subtitles=uploadedFiles/{afname}.srt:fontsdir=.:force_style='Fontsize=18\,FontName=Microsoft Yahei UI'  uploadedFiles/{nfname}_newsubtitle.mp4 -y")
    os.system(f"ffmpeg -i uploadedFiles/{nfname}_newsubtitle.mp4 -i uploadedFiles/{afname}.wav -map 0:v:0 -map 1:a:0 -c:v libx264 -crf 23 -profile:v baseline -level 3.0 -pix_fmt yuv420p -c:a aac -ac 2 -b:a 128k -movflags faststart -y -shortest uploadedFiles/{nfname}_{vfname}.mp4 -y")
    generate_preview(f"uploadedFiles/{nfname}_{vfname}.mp4", f"uploadedFiles/{nfname}_preview.mp4")
    os.remove(f"uploadedFiles/{nfname}_{vfname}.mp4.lock") 

@app.post("/api/combineAudioVideo/{username}")
async def post_combine_audio_video(username, background_tasks: BackgroundTasks, video: str = Form(), subtitle: str = Form(), audio: UploadFile = File(default=None)):
    vfname = video.replace(f"{SITEHOST}api/uploadedFiles/", "").replace(".mp4", "")
    if "_" in vfname:
        vfname = vfname[vfname.find("_")+1:]
    afname = str(uuid.uuid4())
    nfname = str(uuid.uuid4())
    open(f"uploadedFiles/{afname}.wav", "wb").write(audio.file.read())
    open(f"uploadedFiles/{afname}.srt", "w").write(gen_srt(eval(subtitle)))
    background_tasks.add_task(combine, vfname, afname, nfname)
    duration = get_video_duration(f"uploadedFiles/{vfname}.mp4")
    db["UserData"].insert_one({
        "userName": username,
        "createTime": (datetime.utcnow() + timedelta(hours=8)).strftime('创建于%Y年%m月%d日\n%H时%M分%S秒') + "\n时长：" + duration + " 秒",
        "previewUrl": f"{SITEHOST}api/uploadedFiles/{nfname}_preview.mp4",
        "videoUrl": f"{SITEHOST}api/uploadedFiles/{nfname}_{vfname}.mp4",
    })
    return {"videoUrl": f"{SITEHOST}api/uploadedFiles/{nfname}_{vfname}.mp4"}

'''
@app.post("/api/combineAudioVideo/{username}")
async def post_combine_audio_video(username, video: str = Form(), subtitle: str = Form(), audio: UploadFile = File(default=None)):
    vfname = video.replace(f"{SITEHOST}api/uploadedFiles/", "").replace(".mp4", "")
    afname = str(uuid.uuid4())
    nfname = str(uuid.uuid4())
    open(f"uploadedFiles/{afname}.wav", "wb").write(audio.file.read())
    open(f"uploadedFiles/{afname}.srt", "w").write(gen_srt(eval(subtitle)))
    if "_" in vfname:
        vfname = vfname[vfname.find("_")+1:]
    if not os.path.exists(f"uploadedFiles/{vfname}_crop.mp4"):
        crop_param = get_crop_param(f"ffmpeg -i uploadedFiles/{vfname}.mp4 -vframes 10 -vf cropdetect -f null -")
        os.system(f"ffmpeg -i uploadedFiles/{vfname}.mp4 -vf {crop_param} -y uploadedFiles/{vfname}_crop.mp4")
    os.system(f"ffmpeg -i uploadedFiles/{vfname}_crop.mp4 -vf \"scale=1280:720:force_original_aspect_ratio=decrease,pad=1280:720:(ow-iw)/2:(oh-ih)/2,setsar=1\" -y uploadedFiles/{nfname}_pad.mp4")
    os.system(f"ffmpeg -i uploadedFiles/{nfname}_pad.mp4 -vf subtitles=uploadedFiles/{afname}.srt:fontsdir=.:force_style='Fontsize=22\,FontName=Microsoft Yahei UI'  uploadedFiles/{nfname}_newsubtitle.mp4 -y")
    os.system(f"ffmpeg -i uploadedFiles/{nfname}_newsubtitle.mp4 -i uploadedFiles/{afname}.wav -map 0:v:0 -map 1:a:0 -c:v libx264 -crf 23 -profile:v baseline -level 3.0 -pix_fmt yuv420p -c:a aac -ac 2 -b:a 128k -movflags faststart -y -shortest uploadedFiles/{nfname}_{vfname}.mp4 -y")
    generate_preview(f"uploadedFiles/{nfname}_{vfname}.mp4", f"uploadedFiles/{nfname}_preview.mp4")
    db["UserData"].insert_one({
        "userName": username,
        "createTime": (datetime.utcnow() + timedelta(hours=8)).strftime('创建于%Y年%m月%d日\n%H时%M分%S秒'),
        "previewUrl": f"{SITEHOST}api/uploadedFiles/{nfname}_preview.mp4",
        "videoUrl": f"{SITEHOST}api/uploadedFiles/{nfname}_{vfname}.mp4",
    })
    return {"videoUrl": f"{SITEHOST}api/uploadedFiles/{nfname}_{vfname}.mp4"}
'''

@app.post("/api/log")
async def post_log(deviceName: Optional[str] = Form(None), message: Optional[str] = Form(None), deviceUniqueIdentifier: Optional[str] = Form(None), createTime: str = Form(), stackTrace: Optional[str] = Form(None), level: Optional[str] = Form(None)):
    with open(f'logs/{deviceUniqueIdentifier}.log', 'a') as logFile:
        logFile.write(f"{createTime} {level} [{deviceName}]: {message}\n")
    with open(f'logs/{deviceUniqueIdentifier}_full.log', 'a') as logFile:
        logFile.write(f"{createTime} {level} [{deviceName}]: {message}\n")
        if stackTrace != None:
            logFile.write(f"{stackTrace}\n")
    return

@app.post("/api/logAudio")
async def post_log_audio(deviceUniqueIdentifier: Optional[str] = Form(None), originalName: Optional[str] = Form(None), audio: UploadFile = File(default=None)):
    if not os.path.exists(os.path.dirname(f'logs/{deviceUniqueIdentifier}_audio/{originalName}')):
        os.makedirs(os.path.dirname(f'logs/{deviceUniqueIdentifier}_audio/{originalName}'))
    with open(f'logs/{deviceUniqueIdentifier}_audio/{originalName}', 'wb') as audioFile:
        audioFile.write(audio.file.read())
    return

@app.post("/api/logResult")
async def post_log_result(deviceUniqueIdentifier: Optional[str] = Form(None), name: Optional[str] = Form(None), data: UploadFile = File(default=None)):
    if not os.path.exists(os.path.dirname(f'logs/{deviceUniqueIdentifier}_result/{name}')):
        os.makedirs(os.path.dirname(f'logs/{deviceUniqueIdentifier}_result/{name}'))
    with open(f'logs/{deviceUniqueIdentifier}_result/{name}', 'wb') as f:
        f.write(data.file.read())
    return

@app.post("/api/logJson")
async def post_log_json(deviceUniqueIdentifier: Optional[str] = Form(None), name: Optional[str] = Form(None), data: UploadFile = File(default=None)):
    if not os.path.exists(os.path.dirname(f'logs/{deviceUniqueIdentifier}_json/{name}')):
        os.makedirs(os.path.dirname(f'logs/{deviceUniqueIdentifier}_json/{name}'))
    with open(f'logs/{deviceUniqueIdentifier}_json/{name}', 'wb') as f:
        f.write(data.file.read())
    return

import subprocess

def get_crop_param(cmd):
    #process = subprocess.Popen(cmd, shell=True,stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    #out, err = process.communicate()
    #errcode = process.returncode
    #err = str(err)
    #pbegin = err.find("crop=")
    #pend = err.find("\\n", pbegin)
    #return err[pbegin:pend]
    #return "crop=1280:450:0:132"
    return "crop=1280:560:0:80"

from string import Template

class DeltaTemplate(Template):
    delimiter = "%"

def strfdelta(tdelta, fmt):
    d = {"D": tdelta.days}
    hours, rem = divmod(tdelta.seconds, 3600)
    minutes, seconds = divmod(rem, 60)
    d["H"] = '{:02d}'.format(hours)
    d["M"] = '{:02d}'.format(minutes)
    d["S"] = '{:02d}'.format(seconds)
    d["ms"] = '{:03d}'.format(int((tdelta.total_seconds() - seconds) * 1000))
    t = DeltaTemplate(fmt)
    return t.substitute(**d)

def tosrttime(k: str):
    return strfdelta(timedelta(milliseconds = int(k)), "%H:%M:%S,%ms")

def gen_srt(subtitle):
    srt = []
    last_k = "0"
    i = 1
    for k, v in subtitle.items():
        srt.append(str(i))
        srt.append(tosrttime(last_k) + " --> " + tosrttime(k))
        srt.append(v + "\n")
        last_k = str(int(k) + 200)
        i += 1
    return "\n".join(srt)

'''
open("uploadedFiles/test_out2.stamp", "w").write("asd")
os.system("ffmpeg -i test_in.mp4 -c:v libx264 -crf 23 -profile:v baseline -level 3.0 -pix_fmt yuv420p -c:a aac -ac 2 -b:a 128k -movflags faststart uploadedFiles/test_out2.mp4 -y")
open("uploadedFiles/test_out2.stamp1", "w").write("asd")
generate_preview("test_in.mp4", "uploadedFiles/test_out2_preview.mp4")
open("uploadedFiles/test_out2.stamp2", "w").write("asd")
print("Process cost:", end - start)
'''


@app.get("/nlp/srl/{testText}")
async def getSenmetic(testText:str):
    global srl
    wordList=[]
    wordList=text2List(testText)
    res=srl(wordList)
    jsonStr=res2Json(res)
    return jsonStr


def text2List(text):
    return text.split(" ")

def res2Json(res):
    sementicList=[]
    for i, pas in enumerate(res):
        turtle=[]
        for form,role,begin,end in pas:
            tempDic={"form":form,
                     "role":role,
                     "begin":begin,
                     "end":end}
            turtle.append(tempDic)
        sementicList.append(turtle)
    dic = {"sementics": sementicList}
    jsonStr=eval(json.dumps(dic,ensure_ascii=False))
    print(jsonStr)
    return jsonStr
