camera_mouse_zoom:
	g++ -Iinclude -Iincl source/* src/camera_mouse_zoom.cpp  -lGL -lglfw -ldl -lX11 -lpthread -o out/camera_mouse_zoom
basic_lighting_specular:
	g++ -Iinclude -Iincl source/* src/basic_lighting_specular.cpp  -lGL -lglfw -ldl -lX11 -lpthread -o out/basic_lighting_specular
materials:
	g++ -Iinclude -Iincl source/* src/materials.cpp  -lGL -lglfw -ldl -lX11 -lpthread -o out/materials