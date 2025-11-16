/**
 * Simple OrbitControls implementation for 3D thumbnails
 * Based on THREE.js OrbitControls but simplified for our use case
 */
console.log('Loading custom OrbitControls implementation');

// Force override any existing OrbitControls to ensure our implementation is used
THREE.OrbitControls = function(camera, domElement) {
    console.log('Creating OrbitControls instance with camera:', camera, 'domElement:', domElement);
    this.camera = camera;
    this.domElement = domElement || document;
    
    // API
    this.enabled = true;
    this.target = new THREE.Vector3();
    
    // Rotation
    this.rotateSpeed = 1.0;
    this.enableRotate = true;
    
    // Zoom
    this.zoomSpeed = 1.0;
    this.enableZoom = true;
    this.minDistance = 0;
    this.maxDistance = Infinity;
    
    // Damping
    this.enableDamping = false;
    this.dampingFactor = 0.25;
    this.dynamicDampingFactor = 0.3;
    
    // Internal state
    var scope = this;
    var rotateStart = new THREE.Vector2();
    var rotateEnd = new THREE.Vector2();
    var rotateDelta = new THREE.Vector2();
    
    var spherical = new THREE.Spherical();
    var sphericalDelta = new THREE.Spherical();
    
    var scale = 1;
    var panOffset = new THREE.Vector3();
    
    var STATE = { NONE: -1, ROTATE: 0, DOLLY: 1, PAN: 2, TOUCH_ROTATE: 3, TOUCH_DOLLY: 4, TOUCH_PAN: 5 };
    var state = STATE.NONE;
    
    // Events
    var changeEvent = { type: 'change' };
    
    function getAutoRotationAngle() {
        return 2 * Math.PI / 60 / 60 * scope.autoRotateSpeed;
    }
    
    function getZoomScale() {
        return Math.pow(0.95, scope.zoomSpeed);
    }
    
    function rotateLeft(angle) {
        sphericalDelta.theta -= angle;
    }
    
    function rotateUp(angle) {
        sphericalDelta.phi -= angle;
    }
    
    function dollyIn(dollyScale) {
        scale /= dollyScale;
    }
    
    function dollyOut(dollyScale) {
        scale *= dollyScale;
    }
    
    function update() {
        var offset = new THREE.Vector3();
        var quat = new THREE.Quaternion().setFromUnitVectors(camera.up, new THREE.Vector3(0, 1, 0));
        var quatInverse = quat.clone().inverse();
        
        var position = scope.camera.position;
        
        offset.copy(position).sub(scope.target);
        offset.applyQuaternion(quat);
        
        spherical.setFromVector3(offset);
        
        spherical.theta += sphericalDelta.theta;
        spherical.phi += sphericalDelta.phi;
        
        spherical.phi = Math.max(0.000001, Math.min(Math.PI - 0.000001, spherical.phi));
        spherical.radius *= scale;
        spherical.radius = Math.max(scope.minDistance, Math.min(scope.maxDistance, spherical.radius));
        
        scope.target.add(panOffset);
        
        offset.setFromSpherical(spherical);
        offset.applyQuaternion(quatInverse);
        
        position.copy(scope.target).add(offset);
        scope.camera.lookAt(scope.target);
        
        if (scope.enableDamping === true) {
            sphericalDelta.theta *= (1 - scope.dampingFactor);
            sphericalDelta.phi *= (1 - scope.dampingFactor);
        } else {
            sphericalDelta.set(0, 0, 0);
        }
        
        scale = 1;
        panOffset.set(0, 0, 0);
        
        scope.dispatchEvent(changeEvent);
    }
    
    function onMouseDown(event) {
        if (scope.enabled === false) return;
        
        event.preventDefault();
        
        if (event.button === 0) {
            state = STATE.ROTATE;
            rotateStart.set(event.clientX, event.clientY);
        }
        
        scope.domElement.addEventListener('mousemove', onMouseMove, false);
        scope.domElement.addEventListener('mouseup', onMouseUp, false);
    }
    
    function onMouseMove(event) {
        if (scope.enabled === false) return;
        
        event.preventDefault();
        
        if (state === STATE.ROTATE) {
            rotateEnd.set(event.clientX, event.clientY);
            rotateDelta.subVectors(rotateEnd, rotateStart);
            
            var element = scope.domElement === document ? scope.domElement.body : scope.domElement;
            
            rotateLeft(2 * Math.PI * rotateDelta.x / element.clientWidth * scope.rotateSpeed);
            rotateUp(2 * Math.PI * rotateDelta.y / element.clientHeight * scope.rotateSpeed);
            
            rotateStart.copy(rotateEnd);
            
            update();
        }
    }
    
    function onMouseUp(event) {
        if (scope.enabled === false) return;
        
        scope.domElement.removeEventListener('mousemove', onMouseMove, false);
        scope.domElement.removeEventListener('mouseup', onMouseUp, false);
        
        state = STATE.NONE;
    }
    
    function onMouseWheel(event) {
        if (scope.enabled === false || scope.enableZoom === false) return;
        
        event.preventDefault();
        event.stopPropagation();
        
        if (event.deltaY < 0) {
            dollyOut(getZoomScale());
        } else if (event.deltaY > 0) {
            dollyIn(getZoomScale());
        }
        
        update();
    }
    
    // Event listeners
    scope.domElement.addEventListener('mousedown', onMouseDown, false);
    scope.domElement.addEventListener('wheel', onMouseWheel, false);
    
    // Public methods
    this.update = update;
    
    this.dispose = function() {
        scope.domElement.removeEventListener('mousedown', onMouseDown, false);
        scope.domElement.removeEventListener('wheel', onMouseWheel, false);
        scope.domElement.removeEventListener('mousemove', onMouseMove, false);
        scope.domElement.removeEventListener('mouseup', onMouseUp, false);
    };
};

THREE.OrbitControls.prototype = Object.create(THREE.EventDispatcher.prototype);
THREE.OrbitControls.prototype.constructor = THREE.OrbitControls;
