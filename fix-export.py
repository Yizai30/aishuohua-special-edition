gradle_file_path = "Export/unityLibrary/build.gradle"
replaced = '''
    implementation fileTree(dir: 'libs', include: ['*.jar'])
    implementation(name: 'NativeGallery', ext:'aar')
    implementation(name: 'androidutils-debug', ext:'aar')
'''
replacer = '''
    implementation fileTree(dir: 'libs', include: ['*.jar', '*.aar'])
'''
build_gradle = open(gradle_file_path, "r").read()
build_gradle = build_gradle.replace(replaced, replacer)
open(gradle_file_path, "w").write(build_gradle)
