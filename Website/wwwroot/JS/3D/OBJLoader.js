/**
 * Simple OBJ Loader
 * Basic implementation for loading OBJ files
 */
THREE.OBJLoader = function(manager) {
    this.manager = (manager !== undefined) ? manager : THREE.DefaultLoadingManager;
    this.materials = null;
};

THREE.OBJLoader.prototype = {
    constructor: THREE.OBJLoader,
    
    setMaterials: function(materials) {
        this.materials = materials;
    },
    
    load: function(url, onLoad, onProgress, onError) {
        var scope = this;
        var loader = new THREE.FileLoader(scope.manager);
        loader.setPath(this.path);
        
        loader.load(url, function(text) {
            try {
                var object = scope.parse(text);
                if (onLoad) onLoad(object);
            } catch (e) {
                if (onError) onError(e);
            }
        }, onProgress, onError);
    },
    
    parse: function(text) {
        var object = new THREE.Group();
        var geometry = new THREE.BufferGeometry();
        
        var vertices = [];
        var normals = [];
        var uvs = [];
        var indices = [];
        
        var lines = text.split('\n');
        var currentMaterial = null;
        
        for (var i = 0; i < lines.length; i++) {
            var line = lines[i].trim();
            if (line.length === 0 || line.charAt(0) === '#') continue;
            
            var parts = line.split(/\s+/);
            var type = parts[0];
            
            if (type === 'v') {
                // Vertex
                vertices.push(
                    parseFloat(parts[1]),
                    parseFloat(parts[2]),
                    parseFloat(parts[3])
                );
            } else if (type === 'vn') {
                // Normal
                normals.push(
                    parseFloat(parts[1]),
                    parseFloat(parts[2]),
                    parseFloat(parts[3])
                );
            } else if (type === 'vt') {
                // UV
                uvs.push(
                    parseFloat(parts[1]),
                    parseFloat(parts[2])
                );
            } else if (type === 'f') {
                // Face
                for (var j = 1; j < parts.length; j++) {
                    var face = parts[j].split('/');
                    var vertexIndex = parseInt(face[0]) - 1;
                    indices.push(vertexIndex);
                }
            } else if (type === 'usemtl') {
                // Material
                currentMaterial = parts[1];
            }
        }
        
        // Create geometry
        geometry.setAttribute('position', new THREE.Float32BufferAttribute(vertices, 3));
        if (normals.length > 0) {
            geometry.setAttribute('normal', new THREE.Float32BufferAttribute(normals, 3));
        } else {
            geometry.computeVertexNormals();
        }
        if (uvs.length > 0) {
            geometry.setAttribute('uv', new THREE.Float32BufferAttribute(uvs, 2));
        }
        geometry.setIndex(indices);
        
        // Create material
        var material = new THREE.MeshLambertMaterial({ color: 0xcccccc });
        if (this.materials && currentMaterial && this.materials.materials[currentMaterial]) {
            material = this.materials.materials[currentMaterial];
        }
        
        // Create mesh
        var mesh = new THREE.Mesh(geometry, material);
        object.add(mesh);
        
        return object;
    }
};
