camera_mouse_zoom:
	g++ -Iinclude -Iincl source/* src/camera_mouse_zoom.cpp  -lGL -lglfw -ldl -lX11 -lpthread -o out/camera_mouse_zoom
basic_lighting_specular:
	g++ -Iinclude -Iincl source/* src/basic_lighting_specular.cpp  -lGL -lglfw -ldl -lX11 -lpthread -o out/basic_lighting_specular
materials:
	g++ -Iinclude -Iincl source/* src/materials.cpp  -lGL -lglfw -ldl -lX11 -lpthread -o out/materials
lighting_maps_specular:
	g++ -Iinclude -Iincl source/* src/lighting_maps_specular.cpp  -lGL -lglfw -ldl -lX11 -lpthread -o out/lighting_maps_specular
light_casters_directional:
	g++ -Iinclude -Iincl source/* src/Light\ casters/light_casters_directional.cpp  -lGL -lglfw -ldl -lX11 -lpthread -o out/light_casters_directional
light_casters_point:
	g++ -Iinclude -Iincl source/* src/Light\ casters/light_casters_point.cpp  -lGL -lglfw -ldl -lX11 -lpthread -o out/light_casters_point
light_casters_spot_soft:
	g++ -Iinclude -Iincl source/* src/Light\ casters/light_casters_spot_soft.cpp  -lGL -lglfw -ldl -lX11 -lpthread -o out/light_casters_spot_soft