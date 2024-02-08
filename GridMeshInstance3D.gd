extends MeshInstance3D

func _ready():
	var material = ShaderMaterial.new()
	var shader_code = """
	shader_type spatial;
	varying vec3 world_pos;

	void vertex() {
		world_pos = (WORLD_MATRIX * vec4(VERTEX, 1.0)).xyz;
	}

	void fragment() {
		vec2 grid_pos = mod(world_pos.xz, 10.0); // Grid size, adjust as necessary
		float line_width = 0.1; // Line width, adjust as necessary
		bool is_grid_line = grid_pos.x < line_width || grid_pos.y < line_width;
		ALBEDO = is_grid_line ? vec3(1.0) : vec3(0.0, 0.0, 0.0); // Line color: white, Background: black
	}
	"""
	material.shader.code = shader_code
	self.material_override = material
