/**
 * Simple OBJ/MTL Loader for local files
 * Simplified version that works with local file paths instead of Roblox CDN hashes
 */
console.log('Loading SimpleOBJMTLLoader implementation');

THREE.SimpleOBJMTLLoader = function() {
    this.manager = THREE.DefaultLoadingManager;
};

THREE.SimpleOBJMTLLoader.prototype = {
    constructor: THREE.SimpleOBJMTLLoader,
    
    load: function(objUrl, mtlUrl, onLoad, onProgress, onError) {
        var scope = this;
        
        // Load MTL file manually to avoid compatibility issues
        var mtlLoader = new THREE.FileLoader(this.manager);
        
        mtlLoader.load(mtlUrl, function(mtlText) {
            try {
                // Parse MTL manually
                var materials = scope.parseMTL(mtlText);
                
                // Load OBJ file
                var objLoader = new THREE.FileLoader(scope.manager);
                objLoader.load(objUrl, function(objText) {
                    try {
                        var object = scope.parseOBJ(objText, materials);
                        if (onLoad) onLoad(object, materials);
                    } catch (e) {
                        console.error('Error parsing OBJ:', e);
                        if (onError) onError(e);
                    }
                }, onProgress, onError);
                
            } catch (e) {
                console.error('Error parsing MTL:', e);
                if (onError) onError(e);
            }
        }, onProgress, onError);
    },
    
    parseMTL: function(text) {
        var materials = {};
        var currentMaterial = null;
        
        var lines = text.split('\n');
        for (var i = 0; i < lines.length; i++) {
            var line = lines[i].trim();
            if (line.length === 0 || line.charAt(0) === '#') continue;
            
            var parts = line.split(/\s+/);
            var type = parts[0];
            
            if (type === 'newmtl') {
                currentMaterial = parts[1];
                materials[currentMaterial] = new THREE.MeshLambertMaterial();
            } else if (currentMaterial && materials[currentMaterial]) {
                var material = materials[currentMaterial];
                
                if (type === 'Kd') {
                    // Diffuse color
                    material.color.setRGB(parseFloat(parts[1]), parseFloat(parts[2]), parseFloat(parts[3]));
                } else if (type === 'Ka') {
                    // Ambient color - ignore for now
                } else if (type === 'Ks') {
                    // Specular color - ignore for now
                } else if (type === 'd' || type === 'Tr') {
                    // Transparency
                    var alpha = parseFloat(parts[1]);
                    if (alpha < 1.0) {
                        material.transparent = true;
                        material.opacity = alpha;
                    }
                }
            }
        }
        
        return materials;
    },
    
    parseOBJ: function(text, materials) {
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
                // Face - convert to triangles
                var faceVertices = [];
                for (var j = 1; j < parts.length; j++) {
                    var face = parts[j].split('/');
                    var vertexIndex = parseInt(face[0]) - 1;
                    faceVertices.push(vertexIndex);
                }
                
                // Triangulate if quad
                if (faceVertices.length === 4) {
                    indices.push(faceVertices[0], faceVertices[1], faceVertices[2]);
                    indices.push(faceVertices[0], faceVertices[2], faceVertices[3]);
                } else if (faceVertices.length === 3) {
                    indices.push(faceVertices[0], faceVertices[1], faceVertices[2]);
                }
            } else if (type === 'usemtl') {
                // Material
                currentMaterial = parts[1];
            }
        }
        
        // Create geometry - use older THREE.js API for compatibility
        if (typeof THREE.BufferGeometry !== 'undefined' && geometry.setAttribute && typeof THREE.Float32BufferAttribute !== 'undefined') {
            // Modern THREE.js (r125+)
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
        } else {
            // Older THREE.js - use regular Geometry instead
            geometry = new THREE.Geometry();
            
            // Add vertices
            for (var i = 0; i < vertices.length; i += 3) {
                geometry.vertices.push(new THREE.Vector3(vertices[i], vertices[i + 1], vertices[i + 2]));
            }
            
            // Add faces
            for (var i = 0; i < indices.length; i += 3) {
                var face = new THREE.Face3(indices[i], indices[i + 1], indices[i + 2]);
                geometry.faces.push(face);
            }
            
            // Add UVs if available
            if (uvs.length > 0) {
                geometry.faceVertexUvs[0] = [];
                for (var i = 0; i < indices.length; i += 3) {
                    var uvFace = [];
                    for (var j = 0; j < 3; j++) {
                        var uvIndex = indices[i + j] * 2;
                        if (uvIndex < uvs.length) {
                            uvFace.push(new THREE.Vector2(uvs[uvIndex], uvs[uvIndex + 1]));
                        } else {
                            uvFace.push(new THREE.Vector2(0, 0));
                        }
                    }
                    geometry.faceVertexUvs[0].push(uvFace);
                }
            }
            
            geometry.computeFaceNormals();
            geometry.computeVertexNormals();
        }
        
        // Create material
        var material = new THREE.MeshLambertMaterial({ color: 0xcccccc });
        if (materials && currentMaterial && materials[currentMaterial]) {
            material = materials[currentMaterial];
        }
        
        // Create mesh
        var mesh = new THREE.Mesh(geometry, material);
        object.add(mesh);
        
        return object;
    }
};

// Also create a fallback OBJMTLLoader that uses the simple implementation
if (!THREE.OBJMTLLoader) {
    THREE.OBJMTLLoader = THREE.SimpleOBJMTLLoader;
}
