// MadStatus.js
MadStatus = {
    running: !1,
    init: function(n, t, i, r) {
        (MadStatus.running || MadStatus.timeout || MadStatus.resumeTimeout) && MadStatus.stop(), MadStatus.updateInterval = i ? i : 2e3, MadStatus.fadeInterval = r ? r : 1e3, MadStatus.timeout = null, MadStatus.resumeTimeout = null, MadStatus.running = !0, MadStatus.field = n, MadStatus.backBuffer = t, MadStatus.field.show(), MadStatus.backBuffer.hide()
    },
    newLib: function() {
        return MadStatus.participle[Math.floor(Math.random() * MadStatus.participle.length)] + " " + MadStatus.modifier[Math.floor(Math.random() * MadStatus.modifier.length)] + " " + MadStatus.subject[Math.floor(Math.random() * MadStatus.subject.length)] + "..."
    },
    start: function() {
        MadStatus.timeout == null && (MadStatus.timeout = setInterval("MadStatus.update()", MadStatus.updateInterval), MadStatus.running = !0)
    },
    stop: function(n) {
        clearInterval(MadStatus.timeout), MadStatus.timeout = null, clearTimeout(MadStatus.resumeTimeout), MadStatus.resumeTimeout = null, MadStatus.field[0].innerHTML = typeof n != typeof undefined ? n : "", MadStatus.running = !1
    },
    manualUpdate: function(n, t, i) {
        (MadStatus.timeout || MadStatus.resumeTimeout) && MadStatus.stop(), this.update(n, i), t && (MadStatus.resumeTimeout = setTimeout("MadStatus.start()", 1e3))
    },
    update: function(n, t) {
        (MadStatus.backBuffer[0].innerHTML = typeof n != typeof undefined ? n : this.newLib(), typeof t == typeof undefined || t != !1) && (this.field.hide(), this.backBuffer.fadeIn(this.fadeInterval + 2, function() {
            MadStatus.field[0].innerHTML = MadStatus.backBuffer[0].innerHTML, MadStatus.field.show(), MadStatus.backBuffer.hide()
        }))
    },
    participle: ["Accelerating", "Aggregating", "Allocating", "Acquiring", "Automating", "Backtracing", "Bloxxing", "Bootstrapping", "Calibrating", "Correlating", "De-noobing", "De-ionizing", "Deriving", "Energizing", "Filtering", "Generating", "Indexing", "Loading", "Noobing", "Optimizing", "Oxidizing", "Queueing", "Parsing", "Processing", "Rasterizing", "Reading", "Registering", "Re-routing", "Resolving", "Sampling", "Updating", "Writing"],
    modifier: ["Blox", "Count Zero", "Cylon", "Data", "Ectoplasm", "Encryption", "Event", "Farnsworth", "Bebop", "Flux Capacitor", "Fusion", "Game", "Gibson", "Host", "Mainframe", "Metaverse", "Nerf Herder", "Neutron", "Noob", "Photon", "Profile", "Script", "Skynet", "TARDIS", "Virtual"],
    subject: ["Analogs", "Blocks", "Cannon", "Channels", "Core", "Database", "Dimensions", "Directives", "Engine", "Files", "Gear", "Index", "Layer", "Matrix", "Paradox", "Parameters", "Parsecs", "Pipeline", "Players", "Ports", "Protocols", "Reactors", "Sphere", "Stream", "Switches", "Table", "Targets", "Throttle", "Tokens", "Torpedoes", "Tubes"]
};