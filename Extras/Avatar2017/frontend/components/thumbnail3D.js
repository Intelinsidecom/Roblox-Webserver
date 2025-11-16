import { wait } from "../lib/utils";

export class Thumbnail3DHandler {
    // Public
    /** @type {Thumbnail3D|null} */
    thumbJson = null;
    /** @type {HTMLElement|null} */
    canvasParent = null;
    isLoadingThumbnail = false;
    set3DReady = null; // for usages outside of the function, this is a useState fn., it is ReadyForView
    
    // Private (because JavaScript is lame and doesnt allow setting private, only TS
    /** @type Scene */
    scene = null;
    /** @type WebGLRenderer */
    renderer = null;
    /** @type PerspectiveCamera */
    camera = null;
    orbitControls = null;
    directionalLight = null;
    isStopping = false;
    
    constructor() {
    }
    
    /**
     * @param {number} size
     * @constructor
     */
    Init(size = 352) {
        // Initialize the scene
        this.scene = new THREE.Scene();
        this.renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });
        this.camera = new THREE.PerspectiveCamera(70)
        
        this.renderer.setClearColor(0x000000, 0);
        this.renderer.setSize(size, size);
        this.canvasRef = this.renderer.domElement;
        
        this.scene.add(new THREE.AmbientLight(0x808080, 1));
    }
    
    // Starts the animation loop
    Run = () => {
        if (this.isStopping) {
            if (this.set3DReady) this.set3DReady(false);
            if (this.canvasParent && this.canvasParent.contains(this.renderer.domElement)) this.canvasParent.removeChild(this.renderer.domElement);
            this.isStopping = false;
            return;
        }
        
        this.orbitControls?.update();
        this.renderer?.render(this.scene, this.camera);
        requestAnimationFrame(this.Run);
    }
    
    async Stop() {
        this.isStopping = true;
        if (this.canvasParent && this.canvasParent.contains(this.renderer.domElement)) this.canvasParent.removeChild(this.renderer.domElement);
        if (this.set3DReady) this.set3DReady(false);
        this.isLoadingThumbnail = false;
    }
    
    /**
     * @param {Thumbnail3D} thumbJson
     * @param {HTMLElement} canvasParent
     * @param {import('react').Dispatch<boolean>} set3DReady
     * @constructor
     */
    async LoadThumbnail(thumbJson, canvasParent, set3DReady) {
        await this.Stop();
        if (!this.scene) this.Init();
        set3DReady(false);
        this.isLoadingThumbnail = true;
        this.set3DReady = set3DReady;
        const markedForRemoval = [];
        this.scene.traverse(object => {
            if (!object.isMesh) return;
            
            if (object.geometry) object.geometry.dispose();
            if (object.material) {
                if (Array.isArray(object.material)) {
                    object.material.forEach(material => material.dispose());
                } else {
                    object.material.dispose();
                }
                if (object.material?.map) object.material.map.dispose();
            }
            
            markedForRemoval.push(object);
        });
        for (const mesh of markedForRemoval) {
            if (mesh.parent) mesh.parent.remove(mesh);
        }
        this.renderer.render(this.scene, this.camera); // Call the empty scene
        if (this.canvasParent && this.canvasParent.contains(this.renderer.domElement)) this.canvasParent.removeChild(this.renderer.domElement);
        if (this.directionalLight) this.scene.remove(this.directionalLight);
        if (this.orbitControls) this.orbitControls.dispose();
        this.thumbJson = thumbJson;
        this.canvasParent = canvasParent;
        
        this.canvasParent.appendChild(this.renderer.domElement);
        this.camera.fov = thumbJson.camera.fov;
        this.orbitControls = new THREE.OrbitControls(this.camera, this.renderer.domElement, thumbJson);
        
        this.directionalLight = new THREE.DirectionalLight(0xffffff, 1.2);
        this.directionalLight.position.set(thumbJson.aabb.max.x, thumbJson.aabb.max.y + 5, thumbJson.aabb.max.z + 5);
        this.directionalLight.castShadow = false;
        this.scene.add(this.directionalLight);
        
        let mtlLoader = new THREE.MTLLoader();
        mtlLoader.load(thumbJson.mtl, (materials) => {
            // Set the textures to the textures in the JSON
            if (thumbJson.textures.length > 0) {
                const texArr = thumbJson.textures;
                for (const materialName in materials.materialsInfo) {
                    const info = materials.materialsInfo[materialName];
                    
                    for (const key in info) {
                        if (key.startsWith('map_')) {
                            info[key] = (() => {
                                const texId = materialName.match(/Player\d+/)?.[0];
                                return texArr.find(tex => tex.includes(`${texId}Tex`))
                            })() || "";
                        }
                    }
                }
            }
            materials.preload();
            
            // Now load in the meshes
            let objLoader = new THREE.OBJLoader();
            objLoader.setMaterials(materials);
            objLoader.load(thumbJson.obj, (object) => {
                object.scale.set(1, 1, 1);
                this.scene.add(object);
                
                // Now render the scene, start the rendering loop
                set3DReady(true);
                this.isLoadingThumbnail = false;
                this.isStopping = false; // band-aid fix, should be looked at when isStopped is assigned
                this.Run();
            });
        });
    }
    
    // Clears and disposes of everything in this instancer
    Dispose() {
        if (this.canvasParent && this.canvasParent.contains(this.renderer.domElement)) this.canvasParent.removeChild(this.renderer.domElement);
        if (this.directionalLight) this.scene.remove(this.directionalLight);
        if (this.orbitControls) this.orbitControls.dispose();
        if (this.camera) this.camera.clear();
        if (this.renderer) this.renderer.dispose();
        if (this.scene) {
            const markedForRemoval = [];
            this.scene.traverse(object => {
                if (!object.isMesh) return;
                
                if (object.geometry) object.geometry.dispose();
                if (object.material) {
                    if (Array.isArray(object.material)) {
                        object.material.forEach(material => material.dispose());
                    } else {
                        object.material.dispose();
                    }
                    if (object.material?.map) object.material.map.dispose();
                }
                markedForRemoval.push(object);
            });
            for (const mesh of markedForRemoval) {
                if (mesh.parent) mesh.parent.remove(mesh);
            }
            this.scene.dispose();
        }
        if (this.set3DReady) this.set3DReady(false);
        this.isLoadingThumbnail = false;
    }
}